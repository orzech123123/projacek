using Microsoft.EntityFrameworkCore;
using react_app.Wmprojack.Entities;

namespace react_app.Wmprojack
{
    public class WmprojackDbContext : DbContext
    {
        public WmprojackDbContext(DbContextOptions<WmprojackDbContext> options) : base(options)
        {

        }

        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasIndex(u => new { u.ProviderId, u.ProviderType, u.Code })
                .IsUnique();

            modelBuilder.Entity<Order>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd();
        }
    }
}
