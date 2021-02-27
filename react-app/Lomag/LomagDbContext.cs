using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using react_app.Lomag.Entities;

namespace react_app.Lomag
{
    public class LomagDbContext : DbContext
    {
        private readonly IOptions<LomagSettings> settings;

        public DbSet<Towar> Towars { get; set; }

        public LomagDbContext(IOptions<LomagSettings> settings)
        {
            this.settings = settings;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(settings.Value.ConnectionString);
        }
    }
}
