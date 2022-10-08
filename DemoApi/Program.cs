using DemoApi.Authorizations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// https://github.com/openiddict/openiddict-samples/blob/dev/samples/Zirku/Zirku.Api1/Program.cs
builder.Services.AddOpenIddict().AddValidation(options =>
{
    // Note: the validation handler uses OpenID Connect discovery
    // to retrieve the address of the introspection endpoint.
    options.SetIssuer("http://localhost:5051/");
    options.AddAudiences("demo-api-server");

    // Configure the validation handler to use introspection and register the client
    // credentials used when communicating with the remote introspection endpoint.
    options.UseIntrospection()
           .SetClientId("demo-api-server")
           .SetClientSecret("HelloWorld1!");

    // Register the System.Net.Http integration.
    options.UseSystemNetHttp();

    // Register the ASP.NET Core host.
    options.UseAspNetCore();
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthorizationPolicyNames.ScopeDemoApi, AuthorizationPolicyFactory.ScopeDemoApi);
    //options.FallbackPolicy = AuthorizationPolicyFactory.ScopeDemoApi;
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
