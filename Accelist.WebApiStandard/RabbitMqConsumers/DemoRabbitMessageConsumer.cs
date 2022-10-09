using Accelist.WebApiStandard.Contracts.RequestModels;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Accelist.WebApiStandard.RabbitMqConsumers
{
    public class DemoRabbitMessageConsumer : IConsumer<DemoRabbitMessage>
    {
        private readonly ILogger<DemoRabbitMessageConsumer> _logger;

        public DemoRabbitMessageConsumer(ILogger<DemoRabbitMessageConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<DemoRabbitMessage> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Hello from RabbitMQ! {A} + {B} = {C}", msg.A, msg.B, msg.A + msg.B);
            return Task.CompletedTask;
        }
    }
}
