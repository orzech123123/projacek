using Microsoft.EntityFrameworkCore;
using react_app.Lomag;
using react_app.Lomag.Entities;
using System;
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
            return await lomagDbContext.Towary
                .Where(t => t.IdMagazynu == GetMagazyn2022().IdMagazynu)
                .ToListAsync();
        }

        public Kontrahent GetApiloKontrahent() => lomagDbContext.Kontrahenci.Single(k => k.Nazwa == "Apilo");
        public Kontrahent GetWmProjackKontrahent() => lomagDbContext.Kontrahenci.Single(k => k.Nazwa == "Weronika Matecka PROJACK");
        public Magazyn GetMagazyn2022() => lomagDbContext.Magazyny.Single(k => k.Nazwa == "Magazyn 2022");
        public RodzajRuchuMagazynowego GetWydanieZMagazynuRodzajRuchu() =>  lomagDbContext.RodzajeRuchuMagazynowego.Single(k => k.Nazwa == "Wydanie z magazynu");
        public Uzytkownik GetUzytkownik() => lomagDbContext.Uzytkownicy.First();

        public ILookup<T, ElementRuchuMagazynowego> GetWolnePrzyjecia<T>(Func<ElementRuchuMagazynowego, T> byFunc, Towar? towar = null)
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
                .Where(e => byFunc(e) != null)
                .ToLookup(e => byFunc(e));
        }

        public IDictionary<T, int> GetStany<T>(Func<ElementRuchuMagazynowego, T> byFunc, Towar? towar = null)
        {
            var wolnePrzyjecia = GetWolnePrzyjecia(byFunc, towar);

            return wolnePrzyjecia
                .Select(wp => new
                {
                    wp.Key,
                    Stan = (int)wp.Sum(st => st.Ilosc - (st.Wydano ?? 0))
                })
                .ToDictionary(s => s.Key, s => s.Stan);
        }

        public IEnumerable<string> ExtractCodes(string codes, IEnumerable<string> lomagKodyKreskowe, int quantity, Action<string> onInvalidCodeAction)
        {
            if(string.IsNullOrWhiteSpace(codes) || quantity < 1)
            {
                return Enumerable.Empty<string>();
            }

            var allCodes = codes
                .Split(new[] { ' ' })
                .ToList();

            var lomagCodes = allCodes
                .Where(strPart => lomagKodyKreskowe.Contains(strPart))
                .ToList();

            var invalidCodes = allCodes
                .Where(strPart => !lomagKodyKreskowe.Contains(strPart))
                .ToList();

            foreach(var invalidCode in invalidCodes)
            {
                onInvalidCodeAction(invalidCode);
            }

            var resultCodes = new List<string>();

            for (var i = 0; i < quantity; i++)
            {
                resultCodes.AddRange(lomagCodes);
            }

            return resultCodes;
        }
    }
}
