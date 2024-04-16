namespace Accelist.WebApiStandard.WebApi.AuthorizationPolicies
{
    /// <summary>
    /// static class for authorization policy names.
    /// </summary>
    public static class AuthorizationPolicyNames
    {
        /// <summary>
        /// Constant for the scope:api.
        /// </summary>
        public const string ScopeApi = "scope:api";

        /// <summary>
        /// Constant for the scope:api;role:Administrator.
        /// </summary>
        public const string ScopeApiRoleAdministrator = "scope:api;role:Administrator";
    }
}
