using Accelist.WebApiStandard.Contracts.RequestModels;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Accelist.WebApiStandard.RequestHandlers
{
    public class DemoKafkaRequestHandler : AsyncRequestHandler<DemoKafkaRequest>
    {
        private readonly ILogger<DemoKafkaRequestHandler> _logger;

        public DemoKafkaRequestHandler(ILogger<DemoKafkaRequestHandler> logger)
        {
            _logger = logger;
        }

        protected override Task Handle(DemoKafkaRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Hello from Kafka! {A} + {B} = {C}", request.A, request.B, request.A + request.B);

            return Task.CompletedTask;
        }
    }
}
