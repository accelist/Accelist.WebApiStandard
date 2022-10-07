using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Accelist.WebApiStandard.Services.Kafka
{
    public class KafkaJsonProducer
    {
        private readonly IProducer<Null, string> _producer;
        private readonly ILogger<KafkaJsonProducer> _logger;

        public KafkaJsonProducer(IProducer<Null, string> producer, ILogger<KafkaJsonProducer> logger)
        {
            _producer = producer;
            _logger = logger;
        }

        public Task<long?> ProduceAsync<T>(T data, CancellationToken cancellationToken) where T : class
        {
            return ProduceAsync(KafkaTopicNameTools.GetTopicNameFromType<T>(), data, cancellationToken);
        }

        public async Task<long?> ProduceAsync(string topic, object data, CancellationToken cancellationToken)
        {
            try
            {
                var message = new Message<Null, string>
                {
                    Value = JsonSerializer.Serialize(data)
                };

                var result = await _producer.ProduceAsync(topic, message, cancellationToken);
                if (result.Status != PersistenceStatus.Persisted)
                {
                    return null;
                }
                return result.Offset.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred when producing message to Kafka for topic: {Topic}", topic);
                return null;
            }
        }

        public void Produce<T>(T data) where T : class
        {
            Produce(KafkaTopicNameTools.GetTopicNameFromType<T>(), data);
        }

        public void Produce(string topic, object data)
        {
            try
            {
                var message = new Message<Null, string>
                {
                    Value = JsonSerializer.Serialize(data)
                };

                _producer.Produce(topic, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred when producing message to Kafka for topic: {Topic}", topic);
            }
        }
    }
}
