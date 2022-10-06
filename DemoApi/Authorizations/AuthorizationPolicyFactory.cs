using Microsoft.AspNetCore.Authorization;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace DemoApi.Authorizations
{
    public static class AuthorizationPolicyFactory
    {
        public static AuthorizationPolicy ScopeDemoApi => new AuthorizationPolicyBuilder()
            .AddAuthenticationSchemes(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
            .RequireClaim(Claims.Scope, ApiScopeNames.DemoApi)
            .Build();
    }
}
