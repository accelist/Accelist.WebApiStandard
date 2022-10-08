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
    options.WriteErrorLogsToFile = "/Logs/Accelist.WebApiStandard.Microservice.log";
    options.WriteToSentry = true;
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRazorPages();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<AppSettings>(builder.Configuration);
builder.Services.AddApplicationServices(options =>
{
    options.PostgreSqlConnectionString = configuration.GetConnectionString("PostgreSql");
    options.AddWebAppOnlyServices = true;

});
builder.Services.AddMassTransitWithRabbitMq(options =>
{
    options.UseRabbitMQ = true;
});
builder.Services.AddKafka();
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

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
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
