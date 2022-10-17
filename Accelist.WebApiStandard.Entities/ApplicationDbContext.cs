using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace Accelist.WebApiStandard.Entities
{
    public class ApplicationDbContext : IdentityDbContext<User>, IDataProtectionKeyContext
    {
        /// <summary>
        /// https://www.postgresql.org/docs/current/pgtrgm.html
        /// </summary>
        private const string PgTrigramExtension = "pg_trgm";

        private const string PgTrigramIndexOperators = "gin_trgm_ops";

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasPostgresExtension(PgTrigramExtension);

            // Guid max length
            builder.Entity<User>().Property(Q => Q.Id).HasMaxLength(36);
            builder.Entity<User>().Property(Q => Q.PhoneNumber).HasMaxLength(16);
            builder.Entity<OpenIddictEntityFrameworkCoreApplication>().Property(Q => Q.Id).HasMaxLength(36);
            builder.Entity<OpenIddictEntityFrameworkCoreAuthorization>().Property(Q => Q.Id).HasMaxLength(36);
            builder.Entity<OpenIddictEntityFrameworkCoreScope>().Property(Q => Q.Id).HasMaxLength(36);
            builder.Entity<OpenIddictEntityFrameworkCoreToken>().Property(Q => Q.Id).HasMaxLength(36);

            // D:\VS\Accelist.WebApiStandard\Accelist.WebApiStandard\RequestHandlers\ListUserRequestHandler.cs
            builder.Entity<User>().HasIndex(Q => new { Q.GivenName, Q.Id });

            builder.Entity<User>().HasIndex(Q => Q.GivenName)
                .HasMethod("GIN")
                .HasOperators(PgTrigramIndexOperators);

            builder.Entity<User>().HasIndex(Q => Q.FamilyName)
                .HasMethod("GIN")
                .HasOperators(PgTrigramIndexOperators);

            builder.Entity<User>().HasIndex(Q => Q.Email)
                .HasMethod("GIN")
                .HasOperators(PgTrigramIndexOperators);
        }

        public DbSet<Blob> Blobs => Set<Blob>();

        public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
    }
}