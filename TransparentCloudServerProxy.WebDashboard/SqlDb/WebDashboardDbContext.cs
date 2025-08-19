using Microsoft.EntityFrameworkCore;

using TransparentCloudServerProxy.WebDashboard.SqlDb.Models;

namespace TransparentCloudServerProxy.WebDashboard.SqlDb {
    public class WebDashboardDbContext : DbContext {
        public WebDashboardDbContext(DbContextOptions<WebDashboardDbContext> options) : base(options) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<ProxyUser>()
                .HasMany(t => t.UserSavedProxies)
                .WithOne(l => l.ProxyUserOwner)
                .HasForeignKey(l => l.ProxyUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssociatedServer>()
                .HasMany(t => t.AssociatedCredential)
                .WithOne(l => l.AssociatedServer)
                .HasForeignKey(l => l.AssociatedServerId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<AssociatedServer> AssociatedServers { get; set; } = default!;
        public DbSet<ProxyUser> Users { get; set; } = default!;
        public DbSet<SavedProxy> Proxies { get; set; } = default!;
    }
}
