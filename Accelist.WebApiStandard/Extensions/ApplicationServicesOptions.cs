using System.Security.Claims;

namespace Microsoft.Extensions.Hosting
{
    public class ApplicationServicesOptions
    {
        public string PostgreSqlConnectionString { set; get; } = "";

        public bool AddWebAppOnlyServices { set; get; }

        public ClaimsPrincipal? User { set; get; }
    }
}
