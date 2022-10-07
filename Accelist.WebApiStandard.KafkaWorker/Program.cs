using Accelist.WebApiStandard.KafkaWorker.KafkaConsumers;
using Accelist.WebApiStandard.Services;
using Serilog;
using System.Security.Claims;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureSerilogForApplication(options =>
    {
        options.WriteErrorLogsToFile = "/Logs/Accelist.WebApiStandard.KafkaBackgroundService.log";
    })
    .ConfigureServices((ctx, services) =>
    {
        services.Configure<AppSettings>(ctx.Configuration);

        services.AddApplicationServices(options =>
        {
            var configuration = ctx.Configuration;
            options.PostgreSqlConnectionString = configuration.GetConnectionString("PostgreSql");

            var id = new ClaimsIdentity("Accelist.WebApiStandard.KafkaBackgroundService");
            id.AddClaim(new Claim(ClaimTypes.Name, Environment.MachineName));
            options.User = new ClaimsPrincipal(id);
        });

        // Add Kafka Consumers here:
        services.AddHostedService<DemoKafkaRequestConsumer>();
    })
    .Build();

try
{
    Log.Information("Starting worker host");
    await host.RunAsync();
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
