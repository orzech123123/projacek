using Microsoft.EntityFrameworkCore;
using react_app.Lomag.Entities;

namespace react_app.Lomag
{
    public class LomagDbContext : DbContext
    {
        public LomagDbContext(DbContextOptions<LomagDbContext> options) : base(options)
        {

        }

        public DbSet<Towar> Towary { get; set; }
        public DbSet<ElementRuchuMagazynowego> ElementyRuchuMagazynowego { get; set; }
        public DbSet<RuchMagazynowy> RuchyMagazynowe { get; set; }
        public DbSet<RodzajRuchuMagazynowego> RodzajeRuchuMagazynowego { get; set; }
        public DbSet<Uzytkownik> Uzytkownicy { get; set; }
        public DbSet<Magazyn> Magazyny { get; set; }
        public DbSet<Kontrahent> Kontrahenci { get; set; }
        public DbSet<ZaleznosciPZWZ> ZaleznosciPZWZ{ get; set; }
    }
}
