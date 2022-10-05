namespace Accelist.WebApiStandard.Extensions
{
    public class ApplicationServicesOptions
    {
        public string PostgreSqlConnectionString { set; get; } = "";

        public bool EnableAutomaticMigration { set; get; }

        public string OidcSigningKey { set; get; } = "";

        public string OidcEncryptionKey { set; get; } = "";
    }
}
