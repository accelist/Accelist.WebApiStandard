using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Accelist.WebApiStandard.Entities.MigrationsDesigner
{
    internal class DesignTimeApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            optionsBuilder.UseNpgsql(@"Host=localhost;Port=5432;Database=accelist_web_api_standard;Username=postgres;Password=HelloWorld!");
            optionsBuilder.UseOpenIddict();

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
