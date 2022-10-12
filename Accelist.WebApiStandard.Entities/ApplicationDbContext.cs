using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace Accelist.WebApiStandard.Entities
{
    public class ApplicationDbContext : IdentityDbContext<User>, IDataProtectionKeyContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Guid max length
            builder.Entity<User>().Property(Q => Q.Id).HasMaxLength(36);
            builder.Entity<User>().Property(Q => Q.PhoneNumber).HasMaxLength(16);
            builder.Entity<OpenIddictEntityFrameworkCoreApplication>().Property(Q => Q.Id).HasMaxLength(36);
            builder.Entity<OpenIddictEntityFrameworkCoreAuthorization>().Property(Q => Q.Id).HasMaxLength(36);
            builder.Entity<OpenIddictEntityFrameworkCoreScope>().Property(Q => Q.Id).HasMaxLength(36);
            builder.Entity<OpenIddictEntityFrameworkCoreToken>().Property(Q => Q.Id).HasMaxLength(36);

            // D:\VS\Accelist.WebApiStandard\Accelist.WebApiStandard\RequestHandlers\ListUserRequestHandler.cs
            builder.Entity<User>().HasIndex(Q => new { Q.GivenName, Q.Id });

            // https://www.npgsql.org/efcore/mapping/full-text-search.html#method-1-tsvector-column
            builder.Entity<User>().HasGeneratedTsVectorColumn(
                    p => p.SearchVector,
                    "english",  // Text search config
                    p => new { p.GivenName, p.FamilyName, p.Email })  // Included properties
                .HasIndex(p => p.SearchVector)
                .HasMethod("GIN"); // Index method on the search vector (GIN or GIST)
        }

        public DbSet<Blob> Blobs => Set<Blob>();

        public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
    }
}