using Microsoft.EntityFrameworkCore;
using react_app.Lomag;
using react_app.Lomag.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace react_app.Services
{
    public class LomagService
    {
        private readonly LomagDbContext lomagDbContext;

        public LomagService(LomagDbContext lomagDbContext)
        {
            this.lomagDbContext = lomagDbContext;
        }

        public async Task<IList<Towar>> GetTowary()
        {
            return await lomagDbContext.Towary.ToListAsync();
        }

        public Kontrahent GetAllegroKontrahent() => lomagDbContext.Kontrahenci.Single(k => k.Nazwa == "Allegro");
        public Kontrahent GetWmProjackKontrahent() => lomagDbContext.Kontrahenci.Single(k => k.Nazwa == "Weronika Matecka PROJACK");
        public Magazyn GetProjackMagazyn() => lomagDbContext.Magazyny.Single(k => k.Nazwa == "PROJACK");
        public RodzajRuchuMagazynowego GetWydanieZMagazynuRodzajRuchu() =>  lomagDbContext.RodzajeRuchuMagazynowego.Single(k => k.Nazwa == "Wydanie z magazynu");
        public Uzytkownik GetUzytkownik() => lomagDbContext.Uzytkownicy.First();

        public ILookup<int, ElementRuchuMagazynowego> GetWolnePrzyjecia(Towar? towar = null)
        {
            return lomagDbContext.ElementyRuchuMagazynowego
                .Include(e => e.Towar)
                .Include(e => e.RuchMagazynowy)
                .ThenInclude(r => r.RodzajRuchuMagazynowego)
                .Where(e => towar == null || e.Towar.IdTowaru == towar.IdTowaru)
                .Where(e => e.RuchMagazynowy.RodzajRuchuMagazynowego.Nazwa == "Przyjęcie na magazyn")
                .Where(e => e.Ilosc != null)
                .ToList()
                .Where(e => e.Wydano == null || e.Ilosc - (e.Wydano ?? 0) > 0)
                .OrderByDescending(e => e.Ilosc - (e.Wydano ?? 0))
                .Where(e => e.TowarId.HasValue)
                .ToLookup(e => e.TowarId.Value);
        }
    }
}
