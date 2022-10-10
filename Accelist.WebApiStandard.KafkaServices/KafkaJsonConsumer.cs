using Confluent.Kafka;
using MassTransit.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Accelist.WebApiStandard.KafkaServices
{
    public abstract class KafkaJsonConsumer<T> : BackgroundService where T : class
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly ILogger<KafkaJsonConsumer<T>> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public KafkaJsonConsumer(
            ConsumerConfig config,
            ILogger<KafkaJsonConsumer<T>> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            var consumerBuilder = new ConsumerBuilder<Ignore, string>(config);
            _consumer = consumerBuilder.Build();
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Confluent Kafka recommends One Thread per Consumer Pattern:
            // https://www.confluent.io/blog/kafka-consumer-multi-threaded-messaging/
            // https://www.oreilly.com/library/view/kafka-the-definitive/9781491936153/ch04.html

            new Thread(() =>
            {
                StartConsumerLoop(stoppingToken).GetAwaiter().GetResult();
            }).Start();

            return Task.CompletedTask;
        }

        private string Topic => KafkaTopicNameTools.GetTopicNameFromType<T>();

        private async Task ConsumeJsonMessage(string? message, Offset offset, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(message))
            {
                _logger.LogInformation("Topic {Topic} message offset: {Offset} is null or empty", Topic, offset);
                return;
            }

            var data = JsonSerializer.Deserialize<T>(message);
            if (data == null)
            {
                _logger.LogInformation("Topic {Topic} message offset: {Offset} is null or empty", Topic, offset);
                return;
            }

            _logger.LogInformation("Received topic {Topic} message offset: {Offset}", Topic, offset);

            using var scope = _serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(data, cancellationToken);
        }

        /// <summary>
        /// Official example implementation:
        /// https://github.com/confluentinc/confluent-kafka-dotnet/blob/1.9.x/examples/Web/RequestTimeConsumer.cs
        /// </summary>
        /// <param name="cancellationToken"></param>
        private async Task StartConsumerLoop(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(Topic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(cancellationToken);
                    await ConsumeJsonMessage(result.Message.Value, result.Offset, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException ex)
                {
                    if (ex.Error.IsFatal)
                    {
                        // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                        _logger.LogCritical(ex, "Fatal Kafka consume error: {Reason}", ex.Error.Reason);
                        break;
                    }
                    else
                    {
                        // Consumer errors should generally be ignored (or logged) unless fatal.
                        _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error");
                    break;
                }
            }
        }

        public override void Dispose()
        {
            _consumer.Close(); // Commit offsets and leave the group cleanly.
            _consumer.Dispose();

            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
