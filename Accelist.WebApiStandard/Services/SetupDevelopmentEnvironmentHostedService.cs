using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Accelist.WebApiStandard.Services
{
    public class SetupDevelopmentEnvironmentHostedService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SetupDevelopmentEnvironmentHostedService> _logger;

        public SetupDevelopmentEnvironmentHostedService(IServiceScopeFactory scopeFactory, ILogger<SetupDevelopmentEnvironmentHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var migration = scope.ServiceProvider.GetRequiredService<SetupDevelopmentEnvironmentService>();
                await migration.MigrateAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred when setting up development environment using automatic migration.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
