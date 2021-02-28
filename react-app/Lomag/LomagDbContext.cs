using Microsoft.EntityFrameworkCore;
using react_app.Lomag.Entities;

namespace react_app.Lomag
{
    public class LomagDbContext : DbContext
    {
        public LomagDbContext(DbContextOptions<LomagDbContext> options) : base(options)
        {

        }

        public DbSet<Towar> Towars { get; set; }
    }
}
