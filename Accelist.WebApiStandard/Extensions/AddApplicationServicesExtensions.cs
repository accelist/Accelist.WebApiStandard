using Accelist.WebApiStandard.Entities;
using Accelist.WebApiStandard.Extensions;
using Accelist.WebApiStandard.RequestHandlers;
using Accelist.WebApiStandard.Services;
using Accelist.WebApiStandard.Validators;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Security.Claims;
using System.Security.Cryptography;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Microsoft.AspNetCore.Builder
{
    public static class AddApplicationServicesExtensions
    {
        public static void AddApplicationServices(this WebApplicationBuilder builder, Action<ApplicationServicesOptions> optionsBuilder)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext();

            loggerConfiguration.WriteTo.File("/Logs/Accelist.WebApiStandard.log", LogEventLevel.Warning, rollingInterval: RollingInterval.Day);

            if (builder.Environment.IsDevelopment())
            {
                loggerConfiguration.WriteTo.Console();
            }
            else
            {
                // when running in cloud environments such as Kubernetes, pod logs can be collected by agents,
                // for example: https://docs.logz.io/shipping/log-sources/fluent-bit.html
                loggerConfiguration.WriteTo.Console(new CompactJsonFormatter());
            }

            Log.Logger = loggerConfiguration.CreateLogger();

            var opts = new ApplicationServicesOptions();
            optionsBuilder(opts);

            builder.Host.UseSerilog();
            var services = builder.Services;

            services.AddApplicationDbContext(opts.PostgreSqlConnectionString);

            services.AddDataProtection().PersistKeysToDbContext<ApplicationDbContext>();

            services.AddIdentity<User, IdentityRole>(options =>
            {
                // Configure Identity to use the same JWT claims as OpenIddict instead
                // of the legacy WS-Federation claims it uses by default (ClaimTypes),
                // which saves you from doing the mapping in your authorization controller.
                options.ClaimsIdentity.UserNameClaimType = Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = Claims.Role;
                options.Stores.MaxLengthForKeys = 128;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.User.RequireUniqueEmail = true;
                //options.Password.RequiredLength = 9;
            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            services.AddScoped<CustomSignInManager>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.HttpOnly = HttpOnlyPolicy.Always;
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
                options.Secure = CookieSecurePolicy.SameAsRequest;
            });

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.Configure<AppSettings>(builder.Configuration);
            services.AddTransient(di => di.GetRequiredService<IOptions<AppSettings>>().Value);

            services.AddQuartz(options =>
            {
                options.UseMicrosoftDependencyInjectionJobFactory();
                options.UseSimpleTypeLoader();
                options.UseInMemoryStore();
            });

            services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

            services.AddOpenIddict()
                // Register the OpenIddict core components.
                .AddCore(options =>
                {
                    // Configure OpenIddict to use the Entity Framework Core stores and models.
                    // Note: call ReplaceDefaultEntities() to replace the default entities.
                    options.UseEntityFrameworkCore()
                           .UseDbContext<ApplicationDbContext>();

                    // Configure OpenIddict to use Quartz to prune revoked/expired token.
                    options.UseQuartz(builder =>
                    {
                        // Configure the minimum lifespan tokens must have to be pruned.
                        builder.Configure(o =>
                        {
                            o.MinimumTokenLifespan = TimeSpan.FromDays(7);
                        });
                    });
                })
                // Register the OpenIddict server components.
                .AddServer(options =>
                {
                    // Enable the authorization, token, introspection and userinfo endpoints.
                    options.SetAuthorizationEndpointUris(OpenIdSettings.Endpoints.Authorization)
                           .SetTokenEndpointUris(OpenIdSettings.Endpoints.Token)
                           .SetIntrospectionEndpointUris(OpenIdSettings.Endpoints.Introspection)
                           .SetUserinfoEndpointUris(OpenIdSettings.Endpoints.Userinfo)
                           .SetRevocationEndpointUris(OpenIdSettings.Endpoints.Revoke)
                           .SetIntrospectionEndpointUris(OpenIdSettings.Endpoints.Introspect)
                           .SetLogoutEndpointUris(OpenIdSettings.Endpoints.Logout);

                    // Enable the client credentials flow for machine to machine auth.
                    options.AllowClientCredentialsFlow();

                    // Enable the authorization code flow and refresh token flow for native and web apps.
                    options.AllowAuthorizationCodeFlow();
                    options.AllowRefreshTokenFlow();

                    // Expose all the supported claims in the discovery document.
                    options.RegisterClaims(OpenIdSettings.Claims);

                    // Expose all the supported scopes in the discovery document.
                    options.RegisterScopes(OpenIdSettings.Scopes);

                    // Register the signing and encryption credentials.

                    var signingRsa = RSA.Create();
                    signingRsa.ImportRSAPrivateKey(Convert.FromBase64String(opts.OidcSigningKey), out _);
                    var encryptionRsa = RSA.Create();
                    encryptionRsa.ImportRSAPrivateKey(Convert.FromBase64String(opts.OidcEncryptionKey), out _);

                    options.AddSigningKey(new RsaSecurityKey(signingRsa))
                        .AddEncryptionKey(new RsaSecurityKey(encryptionRsa));

                    // Register the ASP.NET Core host and configure the ASP.NET Core options.
                    options.UseAspNetCore()
                           .DisableTransportSecurityRequirement()
                           .EnableAuthorizationEndpointPassthrough()
                           .EnableTokenEndpointPassthrough()
                           .EnableUserinfoEndpointPassthrough()
                           .EnableLogoutEndpointPassthrough();

                    // Create Data Protection tokens instead of JWT tokens.
                    // ASP.NET Core Data Protection uses its own key ring to encrypt and protect tokens against tampering
                    // and is supported for all types of tokens, except identity tokens, that are always JWT tokens.
                    options.UseDataProtection();

                    // Configures OpenIddict to use reference tokens, so that the access token payloads
                    // are stored in the database (only an identifier is returned to the client application).
                    options.UseReferenceAccessTokens()
                        .UseReferenceRefreshTokens();

                    options.SetAccessTokenLifetime(TimeSpan.FromHours(24));
                    options.SetRefreshTokenLifetime(TimeSpan.FromDays(30));
                    options.SetRefreshTokenReuseLeeway(TimeSpan.FromSeconds(60));
                })
                // Register the OpenIddict validation components.
                .AddValidation(options =>
                {
                    // Import the configuration from the local OpenIddict server instance.
                    options.UseLocalServer();

                    // Enable authorization entry validation, which is required to be able
                    // to reject access tokens retrieved from a revoked authorization code.
                    options.EnableAuthorizationEntryValidation();

                    // Enables token validation so that a database call is made for each API request,
                    // required when the OpenIddict server is configured to use reference tokens.
                    options.EnableTokenEntryValidation();

                    // Register the ASP.NET Core host.
                    options.UseAspNetCore();
                    options.UseDataProtection();
                });

            if (opts.AddUserFromHttpContext)
            {
                builder.Services.AddHttpContextAccessor();
                builder.Services.AddScoped(di =>
                {
                    var contextAccessor = di.GetRequiredService<IHttpContextAccessor>();
                    return contextAccessor.HttpContext?.User ?? new ClaimsPrincipal();
                });
            }
            else
            {
                builder.Services.AddScoped(di => opts.User ?? new ClaimsPrincipal());
            }

            services.AddHealthChecks()
                .AddNpgSql(opts.PostgreSqlConnectionString);

            services.AddMediatR(typeof(CreateUserRequestHandler));
            services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();

            if (opts.EnableAutomaticMigration)
            {
                services.AddScoped<SetupDevelopmentEnvironmentService>();
                services.AddHostedService<SetupDevelopmentEnvironmentHostedService>();
            }
        }
    }
}
