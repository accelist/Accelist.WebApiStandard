using Accelist.WebApiStandard.Entities;
using Accelist.WebApiStandard.Services;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Collections.Immutable;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.WebApiStandard.Microservice.Controllers
{
    public class ConnectController : Controller
    {
        private readonly IOpenIddictScopeManager _scopeManager;
        private readonly CustomSignInManager _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IOpenIddictApplicationManager _applicationManager;

        public ConnectController(
            IOpenIddictScopeManager scopeManager,
            CustomSignInManager signInManager,
            UserManager<User> userManager,
            IOpenIddictApplicationManager appManager,
            ApplicationDbContext applicationDbContext)
        {
            _scopeManager = scopeManager;
            _signInManager = signInManager;
            _userManager = userManager;
            _applicationManager = appManager;
        }

        /// <summary>
        /// https://github.com/openiddict/openiddict-samples/blob/dev/samples/Velusia/Velusia.Server/Controllers/AuthorizationController.cs
        /// </summary>
        [HttpGet("connect/authorize"), HttpPost("connect/authorize"), IgnoreAntiforgeryToken]
        public async Task<IActionResult> Authorize(CancellationToken cancellationToken)
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            // Try to retrieve the user principal stored in the authentication cookie and redirect
            // the user agent to the login page (or to an external provider) in the following cases:
            //
            //  - If the user principal can't be extracted or the cookie is too old.
            //  - If prompt=login was specified by the client application.
            //  - If a max_age parameter was provided and the authentication cookie is not considered "fresh" enough.
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
            if (result == null || !result.Succeeded || request.HasPrompt(Prompts.Login) ||
               request.MaxAge != null && result.Properties?.IssuedUtc != null &&
                DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value))
            {
                // If the client application requested promptless authentication,
                // return an error indicating that the user is not logged in.
                if (request.HasPrompt(Prompts.None))
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.LoginRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                        }));
                }

                // To avoid endless login -> authorization redirects, the prompt=login flag
                // is removed from the authorization request payload before redirecting the user.
                var prompt = string.Join(" ", request.GetPrompts().Remove(Prompts.Login));

                var parameters = Request.HasFormContentType ?
                    Request.Form.Where(parameter => parameter.Key != Parameters.Prompt).ToList() :
                    Request.Query.Where(parameter => parameter.Key != Parameters.Prompt).ToList();

                parameters.Add(KeyValuePair.Create(Parameters.Prompt, new StringValues(prompt)));

                return Challenge(
                    authenticationSchemes: IdentityConstants.ApplicationScheme,
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.PathBase + Request.Path + QueryString.Create(parameters)
                    });
            }

            // Retrieve the profile of the logged in user.
            var user = await _userManager.GetUserAsync(result.Principal) ??
                throw new InvalidOperationException("The user details cannot be retrieved.");

            // Create the claims-based identity that will be used by OpenIddict to generate tokens.
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            // Add the claims that will be persisted in the tokens.
            // To minimize database storage, only put essential claims (User ID, Email, Name, Roles)
            // Other claims can be fetch from the userinfo endpoint
            identity.AddClaim(Claims.Subject, user.Id)
                .AddClaim(Claims.Email, user.Email)
                .AddClaim(Claims.Name, ReadUserFullName(user));

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                identity.AddClaim(Claims.Role, role);
            }

            // The OP-Req-acr_values test consists in sending an "acr_values=1 2" parameter
            // as part of the authorization request. To indicate to the certification client
            // that the "1" reference value was satisfied, an "acr" claim is added.
            if (request.HasAcrValue("1"))
            {
                identity.AddClaim(new Claim(Claims.AuthenticationContextReference, "1"));
            }

            var principal = new ClaimsPrincipal(identity);
            principal.SetScopes(request.GetScopes());
            principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes(), cancellationToken).ToListAsync(cancellationToken));

            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(GetDestinations(claim, principal, request));
            }

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// https://github.com/openiddict/openiddict-samples/blob/dev/samples/Velusia/Velusia.Server/Controllers/AuthorizationController.cs
        /// </summary>
        [HttpPost("connect/token"), IgnoreAntiforgeryToken, Produces("application/json")]
        public async Task<IActionResult> Exchange(CancellationToken cancellationToken)
        {
            var request = HttpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

            if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
            {
                // Retrieve the claims principal stored in the authorization code/refresh token.
                var principal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal ??
                    throw new InvalidOperationException("The token authentication cannot be retrieved.");

                // Retrieve the user profile corresponding to the authorization code/refresh token.
                var user = await _userManager.FindByIdAsync(principal.GetClaim(Claims.Subject));
                if (user is null)
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                        }));
                }

                // Ensure the user is still allowed to sign in.
                if (!await _signInManager.CanSignInAsync(user))
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                        }));
                }

                foreach (var claim in principal.Claims)
                {
                    claim.SetDestinations(GetDestinations(claim, principal, request));
                }

                // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // https://github.com/openiddict/openiddict-samples/blob/dev/samples/Aridka/Aridka.Server/Controllers/AuthorizationController.cs
            if (request.IsClientCredentialsGrantType() && request.ClientId != null)
            {
                // Note: the client credentials are automatically validated by OpenIddict:
                // if client_id or client_secret are invalid, this action won't be invoked.

                var application = await _applicationManager.FindByClientIdAsync(request.ClientId, cancellationToken) ??
                    throw new InvalidOperationException("The application details cannot be found in the database.");

                // Create the claims-based identity that will be used by OpenIddict to generate tokens.
                var identity = new ClaimsIdentity(
                    authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                    nameType: Claims.Name,
                    roleType: Claims.Role);

                var clientId = await _applicationManager.GetClientIdAsync(application, cancellationToken) ??
                    throw new InvalidOperationException("The application details cannot be found in the database.");
                var displayName = await _applicationManager.GetDisplayNameAsync(application, cancellationToken) ??
                    throw new InvalidOperationException("The application details cannot be found in the database.");

                // Add the claims that will be persisted in the tokens (use the client_id as the subject identifier).
                identity.AddClaim(Claims.Subject, clientId);
                identity.AddClaim(Claims.Name, displayName);

                // Note: In the original OAuth 2.0 specification, the client credentials grant
                // doesn't return an identity token, which is an OpenID Connect concept.
                //
                // As a non-standardized extension, OpenIddict allows returning an id_token
                // to convey information about the client application when the "openid" scope
                // is granted (i.e specified when calling principal.SetScopes()). When the "openid"
                // scope is not explicitly set, no identity token is returned to the client application.

                // Set the list of scopes granted to the client application in access_token.
                var principal = new ClaimsPrincipal(identity);
                principal.SetScopes(request.GetScopes());
                principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes(), cancellationToken).ToListAsync(cancellationToken));

                foreach (var claim in principal.Claims)
                {
                    claim.SetDestinations(GetDestinations(claim, principal, request));
                }

                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            throw new InvalidOperationException("The specified grant type is not supported.");
        }

        /// <summary>
        /// https://github.com/openiddict/openiddict-samples/blob/dev/samples/Contruum/Contruum.Server/Pages/Connect/Authorize.cshtml.cs
        /// </summary>
        /// <param name="claim"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        private static IEnumerable<string> GetDestinations(Claim claim, ClaimsPrincipal principal, OpenIddictRequest request)
        {
            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            return claim.Type switch
            {
                // Note: always include acr and auth_time in the identity token as they must be flowed
                // from the authorization endpoint to the identity token returned from the token endpoint.
                Claims.AuthenticationContextReference or
                Claims.AuthenticationTime
                    => ImmutableArray.Create(Destinations.IdentityToken),

                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                "AspNet.Identity.SecurityStamp" => ImmutableArray.Create<string>(),

                // Note: when an authorization code or access token is returned, don't add the profile, email,
                // phone and address claims to the identity tokens as they are returned from the userinfo endpoint.
                Claims.Subject or
                Claims.Name or
                Claims.Gender or
                Claims.GivenName or
                Claims.MiddleName or
                Claims.FamilyName or
                Claims.Nickname or
                Claims.PreferredUsername or
                Claims.Birthdate or
                Claims.Profile or
                Claims.Picture or
                Claims.Website or
                Claims.Locale or
                Claims.Zoneinfo or
                Claims.UpdatedAt when principal.HasScope(Scopes.Profile) &&
                    !request.HasResponseType(ResponseTypes.Code) &&
                    !request.HasResponseType(ResponseTypes.Token)
                    => ImmutableArray.Create(Destinations.AccessToken, Destinations.IdentityToken),

                Claims.Email or
                Claims.EmailVerified when principal.HasScope(Scopes.Email) &&
                    !request.HasResponseType(ResponseTypes.Code) &&
                    !request.HasResponseType(ResponseTypes.Token)
                    => ImmutableArray.Create(Destinations.AccessToken, Destinations.IdentityToken),

                Claims.PhoneNumber or
                Claims.PhoneNumberVerified when principal.HasScope(Scopes.Phone) &&
                    !request.HasResponseType(ResponseTypes.Code) &&
                    !request.HasResponseType(ResponseTypes.Token)
                    => ImmutableArray.Create(Destinations.AccessToken, Destinations.IdentityToken),

                Claims.Address when principal.HasScope(Scopes.Address) &&
                    !request.HasResponseType(ResponseTypes.Code) &&
                    !request.HasResponseType(ResponseTypes.Token)
                    => ImmutableArray.Create(Destinations.AccessToken, Destinations.IdentityToken),

                Claims.Role when principal.HasScope(Scopes.Roles) &&
                    !request.HasResponseType(ResponseTypes.Code) &&
                    !request.HasResponseType(ResponseTypes.Token)
                    => ImmutableArray.Create(Destinations.AccessToken, Destinations.IdentityToken),

                _ => ImmutableArray.Create(Destinations.AccessToken)
            };
        }

        /// <summary>
        /// https://github.com/openiddict/openiddict-samples/blob/dev/samples/Velusia/Velusia.Server/Controllers/UserinfoController.cs
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
        [HttpGet("connect/userinfo"), HttpPost("connect/userinfo"), Produces("application/json")]
        public async Task<IActionResult> UserInfo()
        {
            var user = await _userManager.FindByIdAsync(User.GetClaim(Claims.Subject));
            if (user is null)
            {
                return Challenge(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The specified access token is bound to an account that no longer exists."
                    }));
            }

            var claims = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
                [Claims.Subject] = user.Id
            };

            if (User.HasScope(Scopes.Email))
            {
                claims[Claims.Email] = user.Email;
                claims[Claims.EmailVerified] = user.EmailConfirmed;
            }

            if (User.HasScope(Scopes.Phone))
            {
                claims[Claims.PhoneNumber] = user.PhoneNumber;
                claims[Claims.PhoneNumberVerified] = user.PhoneNumberConfirmed;
            }

            if (User.HasScope(Scopes.Profile))
            {
                claims[Claims.GivenName] = user.GivenName;
                claims[Claims.FamilyName] = user.FamilyName;
                claims[Claims.Name] = ReadUserFullName(user);

                claims[Claims.Gender] = user.Gender;
                claims[Claims.Picture] = user.Picture;
                claims[Claims.Birthdate] = user.Birthdate;
            }

            if (User.HasScope(Scopes.Roles))
            {
                // role claims should be persisted in the access token (payload stored and encrypted in database)
                claims[Claims.Role] = User.GetClaims(Claims.Role);
            }

            if (User.HasScope(Scopes.Address))
            {
                claims[Claims.Address] = new Dictionary<string, object>
                {
                    [Claims.Country] = user.Country,
                    [Claims.Locality] = user.City,
                    [Claims.Region] = user.Province,
                    [Claims.PostalCode] = user.PostalCode,
                    [Claims.StreetAddress] = user.StreetAddress
                };
            }

            // Note: the complete list of standard claims supported by the OpenID Connect specification
            // can be found here: http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims

            return Ok(claims);
        }

        [HttpGet("connect/logout")]
        public async Task<IActionResult> Logout()
        {
            // Ask ASP.NET Core Identity to delete the local and external cookies created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            await _signInManager.SignOutAsync();

            // Returning a SignOutResult will ask OpenIddict to redirect the user agent
            // to the post_logout_redirect_uri specified by the client application or to
            // the RedirectUri specified in the authentication properties if none was set.
            return SignOut(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = "/"
                });
        }

        private static string ReadUserFullName(User user)
        {
            return string.IsNullOrEmpty(user.FamilyName) ? user.GivenName : $"{user.GivenName} {user.FamilyName}";
        }
    }
}
