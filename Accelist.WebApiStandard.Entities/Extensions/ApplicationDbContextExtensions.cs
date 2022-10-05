using Microsoft.EntityFrameworkCore;
using Accelist.WebApiStandard.Entities;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApplicationDbContextExtensions
    {
        public static void AddApplicationDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContextPool<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
                options.UseOpenIddict();
            });
        }
    }
}
