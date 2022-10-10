using Accelist.WebApiStandard.KafkaServices;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
{
    public static class AddKafkaServicesExtensions
    {
        public static void AddKafkaServices(this IServiceCollection services, Action<KafkaServicesOptions>? optionsBuilder = default)
        {
            var opts = new KafkaServicesOptions();
            optionsBuilder?.Invoke(opts);

            services.AddTransient(di =>
            {
                return new ProducerConfig
                {
                    BootstrapServers = opts.BootstrapServers,
                    ClientId = opts.ClientId
                };
            });

            services.AddTransient(di =>
            {
                return new ConsumerConfig
                {
                    BootstrapServers = opts.BootstrapServers,
                    ClientId = opts.ClientId,
                    GroupId = opts.ConsumerGroup,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    AllowAutoCreateTopics = true
                };
            });

            // Kafka Producer is thread safe: https://github.com/confluentinc/confluent-kafka-dotnet/issues/1041
            // Using Singleton Kafka Producer is approximately 500,000 times more efficient than recreating Producers.
            // (Reference: https://github.com/confluentinc/confluent-kafka-dotnet/issues/1346)
            services.AddSingleton(di =>
            {
                var config = di.GetRequiredService<ProducerConfig>();
                var producerBuilder = new ProducerBuilder<Null, string>(config);
                return producerBuilder.Build();
            });

            services.AddTransient<KafkaJsonProducer>();
        }
    }
}
