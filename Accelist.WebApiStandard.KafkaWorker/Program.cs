using Accelist.WebApiStandard.KafkaWorker.KafkaConsumers;
using Accelist.WebApiStandard.Services;
using Serilog;
using System.Security.Claims;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging((ctx, l) =>
    {
        // https://docs.sentry.io/platforms/dotnet/guides/extensions-logging/
        // The ILoggingBuilder extension is useful when using a HostBuilder, also known as Generic host. 
        // Bind the appsettings.json configuration to make the values available to the Sentry integration.
        // Unfortunately, there are no Sentry integrations for IHostBuilder yet: https://github.com/getsentry/sentry-dotnet/issues/190
        l.AddConfiguration(ctx.Configuration);
        l.AddSentry();
    })
    .ConfigureSerilogWithSentry(options =>
    {
        options.WriteErrorLogsToFile = "/Logs/Accelist.WebApiStandard.KafkaWorker.log";
        options.WriteToSentry = true;
    })
    .ConfigureServices((ctx, services) =>
    {
        services.Configure<AppSettings>(ctx.Configuration);

        services.AddApplicationServices(options =>
        {
            var configuration = ctx.Configuration;
            options.PostgreSqlConnectionString = configuration.GetConnectionString("PostgreSql");

            var id = new ClaimsIdentity("Accelist.WebApiStandard.KafkaWorker");
            id.AddClaim(new Claim(ClaimTypes.Name, Environment.MachineName));
            options.User = new ClaimsPrincipal(id);
        });
        // Add MassTransit just for Mediator functions
        services.AddMassTransitWithRabbitMq(options =>
        {
            options.UseRabbitMQ = false;
        });
        services.AddKafka();

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
