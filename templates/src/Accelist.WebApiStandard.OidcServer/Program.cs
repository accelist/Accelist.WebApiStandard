using Accelist.WebApiStandard.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Sentry automatically loads appsettings.json and environment variables, binding to SentryAspNetCoreOptions
// Don't forget to set TracesSampleRate to lower values (for example, 0.2) in Production environment!
builder.WebHost.UseSentry();
builder.Host.ConfigureSerilogWithSentry(options =>
{
    options.WriteErrorLogsToFile = "/Logs/Accelist.WebApiStandard.OidcServer.log";
    options.WriteToSentry = true;
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorPages();

builder.Services.Configure<AppSettings>(builder.Configuration);
builder.Services.AddApplicationServices(options =>
{
    options.PostgreSqlConnectionString = configuration.GetConnectionString("PostgreSql");
    options.AddWebAppOnlyServices = true;

});
builder.Services.AddOpenIdConnectServer(options => {
    // Use api/generate-rsa-keys to get new random values 
    options.SigningKey = configuration["oidcSigningKey"];
    options.EncryptionKey = configuration["oidcEncryptionKey"];
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEntityFrameworkCoreAutomaticMigrations();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

if (app.Environment.IsDevelopment() == false)
{
    app.UseExceptionHandler("/error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

// Write streamlined request completion events, instead of the more verbose ones from the framework.
// To use the default framework request logging instead, remove this line and set the "Microsoft"
// level in appsettings.json to "Information".
app.UseSerilogRequestLogging();

app.UseRouting();
app.UseCors(options =>
{
    // Allow token endpoint be hit from front-end web apps (e.g. JS with PKCE code flow)
    // https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-6.0#how-cors-works
    // CORS is NOT a security feature.
    // CORS is a W3C standard that allows a server to relax the same-origin policy. 
    // An API isn't safer by allowing CORS. It's up to the client (browser) to enforce CORS.
    // The server executes the request and returns the response, it's the client that returns an error and blocks the response.
    // The most important part here is: CORS is enforced by the BROWSER, not the server.
    // This means that the Access-Control-Allow-Origin protects the browser, not the resource on the server or the server itself!
    // Fetch / AJAX / XHR requests omits cookie from CORS anyway https://fetch.spec.whatwg.org/#cors-protocol-and-credentials 
    options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
});

// https://docs.sentry.io/platforms/dotnet/guides/aspnetcore/performance/instrumentation/automatic-instrumentation
app.UseSentryTracing();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();

try
{
    Log.Information("Starting web host");
    app.Run();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
