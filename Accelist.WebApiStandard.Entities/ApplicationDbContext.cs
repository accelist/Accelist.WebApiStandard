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
        }

        public DbSet<Blob> Blobs => Set<Blob>();

        public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
    }
}