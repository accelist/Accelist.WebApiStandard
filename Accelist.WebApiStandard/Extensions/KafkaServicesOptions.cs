namespace Microsoft.Extensions.Hosting
{
    public class KafkaServicesOptions
    {
        public string BootstrapServers { set; get; } = "localhost:9092";

        public string ClientId { set; get; } = Environment.MachineName;

        public string ConsumerGroup { set; get; } = "group-1";
    }
}
