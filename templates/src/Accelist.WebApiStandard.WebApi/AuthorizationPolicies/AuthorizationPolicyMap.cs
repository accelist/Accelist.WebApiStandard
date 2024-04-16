using Microsoft.AspNetCore.Authorization;
using OpenIddict.Validation.AspNetCore;
using System.Collections.ObjectModel;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Accelist.WebApiStandard.WebApi.AuthorizationPolicies
{
    /// <summary>
    /// Authorization policy mapper class.
    /// </summary>
    public static class AuthorizationPolicyMap
    {
        private static readonly Dictionary<string, AuthorizationPolicy> _map = new()
        {
            [AuthorizationPolicyNames.ScopeApi] = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
                .RequireClaim(Claims.Private.Scope, ApiScopeNames.Api)
                .Build(),

            [AuthorizationPolicyNames.ScopeApiRoleAdministrator] = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
                .RequireClaim(Claims.Private.Scope, ApiScopeNames.Api)
                .RequireRole(Claims.Role, RoleNames.Administrator)
                .Build(),
        };

        /// <summary>
        /// Readonly dictionary of authorization policies.
        /// </summary>
        public static ReadOnlyDictionary<string, AuthorizationPolicy> Map => new(_map);
    }
}
