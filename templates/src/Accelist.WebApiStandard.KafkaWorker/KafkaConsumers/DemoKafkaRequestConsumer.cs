using Accelist.WebApiStandard.Contracts.RequestModels;
using Accelist.WebApiStandard.KafkaServices;
using Confluent.Kafka;

namespace Accelist.WebApiStandard.KafkaWorker.KafkaConsumers
{
    public class DemoKafkaRequestConsumer : KafkaJsonConsumer<DemoKafkaRequest>
    {
        public DemoKafkaRequestConsumer(ConsumerConfig config, ILogger<KafkaJsonConsumer<DemoKafkaRequest>> logger, IServiceScopeFactory serviceScopeFactory) : base(config, logger, serviceScopeFactory)
        {
        }
    }
}
