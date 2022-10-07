using Accelist.WebApiStandard.Entities;
using Accelist.WebApiStandard.RabbitMqConsumers;
using Accelist.WebApiStandard.RequestHandlers;
using Accelist.WebApiStandard.Services;
using Accelist.WebApiStandard.Services.Kafka;
using Accelist.WebApiStandard.Validators;
using Confluent.Kafka;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Serilog;
using Serilog.Events;
using System.Security.Claims;
using System.Security.Cryptography;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Microsoft.Extensions.Hosting
{
    public static class ApplicationBuilderExtensions
    {
        public static IHostBuilder ConfigureSerilogForApplication(this IHostBuilder host, Action<ConfigureSerilogOptions>? optionsBuilder = default)
        {
            var opts = new ConfigureSerilogOptions();
            optionsBuilder?.Invoke(opts);

            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext();

            if (string.IsNullOrEmpty(opts.WriteErrorLogsToFile) == false)
            {
                loggerConfiguration.WriteTo.File(opts.WriteErrorLogsToFile, LogEventLevel.Warning, rollingInterval: RollingInterval.Day);
            }

            loggerConfiguration.WriteTo.Console();

            Log.Logger = loggerConfiguration.CreateLogger();
            return host.UseSerilog();
        }

        public static void AddApplicationServices(this IServiceCollection services, Action<ApplicationServicesOptions>? optionsBuilder = default)
        {
            var opts = new ApplicationServicesOptions();
            optionsBuilder?.Invoke(opts);

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
                // options.Password.RequiredLength = 9;
            }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

            services.AddScoped<CustomSignInManager>();

            //services.Configure<AppSettings>(builder.Configuration);
            //services.AddTransient(di => di.GetRequiredService<IOptions<AppSettings>>().Value);

            if (opts.AddWebAppOnlyServices)
            {
                services.AddHttpContextAccessor();
                services.AddScoped(di =>
                {
                    var contextAccessor = di.GetRequiredService<IHttpContextAccessor>();
                    return contextAccessor.HttpContext?.User ?? new ClaimsPrincipal();
                });

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

                services.AddHealthChecks()
                    .AddNpgSql(opts.PostgreSqlConnectionString);
            }
            else
            {
                services.AddScoped(di => opts.User ?? new ClaimsPrincipal());
            }

            services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
        }

        public static void AddOpenIdConnectServer(this IServiceCollection services, Action<OpenIdConnectServerOptions>? optionsBuilder = default)
        {
            var opts = new OpenIdConnectServerOptions();
            optionsBuilder?.Invoke(opts);

            #region Quartz
            // Quartz.NET is added to support OpenIddict token cleanup
            // However, it is also usable for apps with Cron Job from a Worker Service
            // When doing so, just move Quartz service registration outside `AddOpenIdConnectServer`
            // Then consider adding SQL database persistent storage for Quartz.NET:
            // https://www.quartz-scheduler.net/documentation/quartz-3.x/quick-start.html#creating-and-initializing-database
            // Do not use clustering mode of Quartz.NET unless you know what you're doing... (Just run in a single pod)
            services.AddQuartz(options =>
            {
                options.UseMicrosoftDependencyInjectionJobFactory();
                options.UseSimpleTypeLoader();
                options.UseInMemoryStore();
            });

            services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
            #endregion

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
                        signingRsa.ImportRSAPrivateKey(Convert.FromBase64String(opts.SigningKey), out _);
                        var encryptionRsa = RSA.Create();
                        encryptionRsa.ImportRSAPrivateKey(Convert.FromBase64String(opts.EncryptionKey), out _);

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
        }

        public static void AddEntityFrameworkCoreAutomaticMigrations(this IServiceCollection services)
        {
            services.AddScoped<SetupDevelopmentEnvironmentService>();
            services.AddHostedService<SetupDevelopmentEnvironmentHostedService>();
        }

        public static void AddKafka(this IServiceCollection services, Action<KafkaServicesOptions>? optionsBuilder = default)
        {
            var opts = new KafkaServicesOptions();
            optionsBuilder?.Invoke(opts);

            services.AddTransient(di =>
            {
                return new ProducerConfig
                {
                    BootstrapServers = opts.BootstrapServers,
                    ClientId = opts.ClientId
                };
            });

            services.AddTransient(di =>
            {
                return new ConsumerConfig
                {
                    BootstrapServers = opts.BootstrapServers,
                    ClientId = opts.ClientId,
                    GroupId = opts.ConsumerGroup,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    AllowAutoCreateTopics = true
                };
            });

            // Kafka Producer is thread safe: https://github.com/confluentinc/confluent-kafka-dotnet/issues/1041
            // Using Singleton Kafka Producer is approximately 500,000 times more efficient than recreating Producers.
            // (Reference: https://github.com/confluentinc/confluent-kafka-dotnet/issues/1346)
            services.AddSingleton(di =>
            {
                var config = di.GetRequiredService<ProducerConfig>();
                var producerBuilder = new ProducerBuilder<Null, string>(config);
                return producerBuilder.Build();
            });

            services.AddTransient<KafkaJsonProducer>();
        }

        public static void AddMassTransitWithRabbitMq(this IServiceCollection services, Action<MassTransitRabbitMqOptions>? optionsBuilder = default)
        {
            var opts = new MassTransitRabbitMqOptions();
            optionsBuilder?.Invoke(opts);

            services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();

                // Add Consumers strictly for RabbitMQ
                x.AddConsumersFromNamespaceContaining<DemoRabbitMessageConsumer>();

                x.AddMediator(cfg =>
                {
                    // Add Consumers strictly for Mediator
                    cfg.AddConsumersFromNamespaceContaining<CreateUserRequestHandler>();
                });

                if (opts.UseRabbitMQ)
                {
                    x.UsingRabbitMq((ctx, cfg) =>
                    {
                        cfg.Host(opts.Host, "/", h =>
                        {
                            h.Username(opts.Username);
                            h.Password(opts.Password);
                        });

                        cfg.ConfigureEndpoints(ctx);
                    });
                }
                else
                {
                    x.UsingInMemory((ctx, cfg) =>
                    {
                        cfg.ConfigureEndpoints(ctx);
                    });
                }
            });
        }
    }
}
