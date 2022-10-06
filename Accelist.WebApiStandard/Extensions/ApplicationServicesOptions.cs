using System.Security.Claims;

namespace Microsoft.Extensions.Hosting
{
    public class ApplicationServicesOptions
    {
        public string PostgreSqlConnectionString { set; get; } = "";

        public bool EnableAutomaticMigration { set; get; }

        public string OidcSigningKey { set; get; } = "";

        public string OidcEncryptionKey { set; get; } = "";

        public bool AddWebAppOnlyServices { set; get; }

        public ClaimsPrincipal? User { set; get; } 

        public KafkaServicesOptions KafkaServicesOptions { set; get; } = new KafkaServicesOptions();
    }

    public class KafkaServicesOptions
    {
        public string BootstrapServers { set; get; } = "localhost:9092";

        public string ClientId { set; get; } = Environment.MachineName;

        public string ConsumerGroup { set; get; } = "group-1";
    }
}
