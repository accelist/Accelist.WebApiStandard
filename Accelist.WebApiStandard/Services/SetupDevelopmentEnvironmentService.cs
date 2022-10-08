using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using Accelist.WebApiStandard.Entities;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Accelist.WebApiStandard.Services
{
    public class SetupDevelopmentEnvironmentService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserEmailStore<User> _userStore;
        private readonly IOpenIddictApplicationManager _appManager;
        private readonly IOpenIddictScopeManager _scopeManager;

        public SetupDevelopmentEnvironmentService(
            ApplicationDbContext applicationDbContext,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<User> userStore,
            IOpenIddictApplicationManager openIddictApplicationManager,
            IOpenIddictScopeManager openIddictScopeManager
        )
        {
            _db = applicationDbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _userStore = (IUserEmailStore<User>)userStore;
            _appManager = openIddictApplicationManager;
            _scopeManager = openIddictScopeManager;
        }

        public async Task MigrateAsync(CancellationToken cancellationToken)
        {
            await _db.Database.MigrateAsync(cancellationToken);
            var user = await AddUserAdministrator(cancellationToken);
            await AddAdministratorRole();
            await AddRoleToAdministratorUser(user, "Administrator");

            await CreateApiServerApp(cancellationToken);
            await CreateApiScope(cancellationToken);
            await CreateCmsApp(cancellationToken);
            await CreateDemoClientApp(cancellationToken);
        }

        private async Task<User> AddUserAdministrator(CancellationToken cancellationToken)
        {
            var exist = await _userManager.FindByNameAsync("administrator@accelist.com");
            if (exist != null)
            {
                return exist;
            }

            var user = new User
            {
                GivenName = "Administrator",
                IsEnabled = true
            };
            await _userStore.SetUserNameAsync(user, "administrator@accelist.com", cancellationToken);
            await _userStore.SetEmailAsync(user, "administrator@accelist.com", cancellationToken);
            await _userManager.CreateAsync(user, "HelloWorld1!");

            return user;
        }

        private async Task CreateApiServerApp(CancellationToken cancellationToken)
        {
            var exist = await _appManager.FindByClientIdAsync("api-server", cancellationToken);
            if (exist != null)
            {
                return;
            }

            await _appManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "api-server",
                DisplayName = "API Server",
                Type = ClientTypes.Confidential,
                ClientSecret = "HelloWorld1!",
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.Introspection,
                    Permissions.Endpoints.Revocation,
                    Permissions.GrantTypes.ClientCredentials
                }
            }, cancellationToken);
        }

        private async Task CreateApiScope(CancellationToken cancellationToken)
        {
            var exist = await _scopeManager.FindByNameAsync("api", cancellationToken);
            if (exist != null)
            {
                return;
            }

            await _scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "api",
                DisplayName = "API Scope",
                Resources =
                {
                    "api-server"
                }
            }, cancellationToken);
        }

        private async Task CreateDemoClientApp(CancellationToken cancellationToken)
        {
            var exist = await _appManager.FindByClientIdAsync("demo-m2m", cancellationToken);
            if (exist != null)
            {
                return;
            }

            await _appManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "demo-m2m",
                DisplayName = "Demo Client App (Machine to Machine)",
                Type = ClientTypes.Confidential,
                ClientSecret = "HelloWorld1!",
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.Introspection,
                    Permissions.Endpoints.Revocation,
                    Permissions.GrantTypes.ClientCredentials,
                    Permissions.Prefixes.Scope + "api"
                }
            }, cancellationToken);
        }

        private async Task<string?> CreateCmsApp(CancellationToken cancellationToken)
        {
            var exist = await _appManager.FindByClientIdAsync("cms", cancellationToken);
            if (exist != null)
            {
                return await _appManager.GetIdAsync(exist, cancellationToken);
            }

            var o = await _appManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "cms",
                DisplayName = "CMS (Front-End)",
                RedirectUris = {
                    new Uri("http://localhost:3000/api/auth/callback/oidc"),
                    new Uri("https://oauth.pstmn.io/v1/callback")
                },
                Permissions =
                {
                    Permissions.Endpoints.Token,
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.Revocation,
                    Permissions.ResponseTypes.Code,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Roles,
                    Permissions.Scopes.Phone,
                    Permissions.Scopes.Address,
                    Permissions.Prefixes.Scope + "api"
                },
                Type = ClientTypes.Public
            }, cancellationToken);

            return await _appManager.GetIdAsync(o, cancellationToken);
        }

        private async Task AddAdministratorRole()
        {
            var exist = await _roleManager.RoleExistsAsync("Administrator");
            if (exist)
            {
                return;
            }

            await _roleManager.CreateAsync(new IdentityRole
            {
                Name = "Administrator"
            });
        }

        private async Task AddRoleToAdministratorUser(User user, string roleName)
        {
            if (await _userManager.IsInRoleAsync(user, roleName))
            {
                return;
            }
            await _userManager.AddToRoleAsync(user, roleName);
        }
    }
}
