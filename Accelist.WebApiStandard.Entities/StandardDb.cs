using Microsoft.EntityFrameworkCore;

namespace Accelist.WebApiStandard.Entities
{
    public class StandardDb : DbContext
    {
        public StandardDb(DbContextOptions<StandardDb> dbContextOptions) : base(dbContextOptions)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}