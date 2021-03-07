using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using react_app.Lomag;
using react_app.Lomag.Entities;
using react_app.Wmprojack;
using react_app.Wmprojack.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace react_app.Services
{
    public class OrderService
    {
        private readonly IEnumerable<IOrderProvider> orderProviders;
        private readonly LomagDbContext lomagDbContext;
        private readonly WmprojackDbContext wmprojackDbContext;
        private readonly ILogger<OrderService> logger;

        public OrderService(
            IEnumerable<IOrderProvider> orderProviders,
            LomagDbContext lomagDbContext,
            WmprojackDbContext wmprojackDbContext,
            ILogger<OrderService> logger)
        {
            this.orderProviders = orderProviders;
            this.lomagDbContext = lomagDbContext;
            this.wmprojackDbContext = wmprojackDbContext;
            this.logger = logger;
        }

        public async Task<int> SyncOrdersAsync()
        {
            var projackDbOrders = await wmprojackDbContext.Orders.ToListAsync();
            var lomagTowars = await lomagDbContext.Towary.ToListAsync();

            var recentApiOrders = orderProviders.SelectMany(p => p.GetOrders()).ToList();

            //TODO remove / tests
            /*recentApiOrders.Clear();
            recentApiOrders.Add(new OrderDto
            {
                Date = DateTime.Now,
                Name = "produkt",
                ProviderOrderId = "sztrytkod",
                ProviderType = OrderProvider.Allegro,
                Codes = "00000008",
                Quantity = 1
            });
            /*recentApiOrders.Add(new Order
            {
                Date = DateTime.Now,
                Name = "zamówienie z 4 w sumie sztukami",
                ProviderOrderId = "96666666-apaczkowykodsztuczny",
                ProviderType = OrderProvider.Apaczka,
                Code = "00000006 00000053 xxxxxxx 00000072 00000053",
                Quantity = 1
            });*/

            var ordersToSync = GetOrdersToSync(projackDbOrders, recentApiOrders, lomagTowars);

            if (!ordersToSync.Any())
            {
                return 0;
            }

            AddOrdersToDbs(ordersToSync, lomagTowars);

            wmprojackDbContext.SaveChanges();
            lomagDbContext.SaveChanges();

            return ordersToSync.Count();
        }

        private IList<Order> GetOrdersToSync(IEnumerable<Order> existingOrders, IEnumerable<OrderDto> recentApiOrders, IEnumerable<Towar> lomagTowars)
        {
            var lomagKodyKreskowe = lomagTowars.Select(t => t.KodKreskowy);
            var ordersToSync = new List<Order>();

            var apiOrdersGroups = recentApiOrders
                .Where(o => o.IsValid)
                .GroupBy(o => new { o.ProviderOrderId, o.ProviderType });

            foreach (var apiOrderGroup in apiOrdersGroups)
            {
                var isDuplicate = existingOrders
                    .Union(ordersToSync)
                    .Any(o => o.ProviderOrderId == apiOrderGroup.Key.ProviderOrderId &&
                              o.ProviderType == apiOrderGroup.Key.ProviderType);

                if (isDuplicate)
                {
                    continue;
                }

                foreach (var apiOrder in apiOrderGroup)
                {
                    var detectedCodes = apiOrder.Codes.Split(new[] { ' ' })
                        .Where(strPart => lomagKodyKreskowe.Contains(strPart));

                    for (var i = 0; i < apiOrder.Quantity; i++)
                    {
                        foreach (var code in detectedCodes)
                        {
                            var name = $"{lomagTowars.Single(t => t.KodKreskowy == code).Nazwa} - {apiOrder.Name}";

                            ordersToSync.Add(new Order
                            {
                                Code = code,
                                Date = apiOrder.Date,
                                Name = name,
                                ProviderOrderId = apiOrder.ProviderOrderId,
                                ProviderType = apiOrder.ProviderType
                            });
                        }
                    }
                }
            }

            return ordersToSync;
        }

        private void AddOrdersToDbs(IEnumerable<Order> ordersToSync, IEnumerable<Towar> lomagTowars)
        {
            var allegroKontrahent = lomagDbContext.Kontrahenci.Single(k => k.Nazwa == "Allegro");
            var wmprojackKontrahent = lomagDbContext.Kontrahenci.Single(k => k.Nazwa == "Weronika Matecka PROJACK");
            var wmprojackMagazyn = lomagDbContext.Magazyny.Single(k => k.Nazwa == "PROJACK");
            var wydanieRodzajRuchu = lomagDbContext.RodzajeRuchuMagazynowego.Single(k => k.Nazwa == "Wydanie z magazynu");
            var user = lomagDbContext.Uzytkownicy.First();

            foreach (var order in ordersToSync)
            {
                var date = order.Date; 

                var ruchMagazynowy = new RuchMagazynowy
                {
                    Kontrahent = allegroKontrahent,
                    Company = wmprojackKontrahent,
                    Data = date,
                    Utworzono = date,
                    Zmodyfikowano = date,
                    Magazyn = wmprojackMagazyn,
                    NrDokumentu = $"WZ/AUTO/{order.ProviderOrderId}",
                    RodzajRuchuMagazynowego = wydanieRodzajRuchu,
                    Uzytkownik = user,
                    Operator = -1
                };

                var towar = lomagTowars.Single(t => t.KodKreskowy == order.Code);

                var uwagi = order.GetUrl();

                var wydanieElementRuchu = new ElementRuchuMagazynowego
                {
                    Towar = towar,
                    CenaJednostkowa = 0,
                    Ilosc = 1,
                    RuchMagazynowy = ruchMagazynowy,
                    Utworzono = date,
                    Zmodyfikowano = date,
                    Uzytkownik = user,
                    Uwagi = uwagi
                };

                var przyjecieElementRuchu = lomagDbContext.ElementyRuchuMagazynowego
                    .Where(e => e.Towar.IdTowaru == towar.IdTowaru)
                    .Where(e => e.RuchMagazynowy.RodzajRuchuMagazynowego.Nazwa == "Przyjęcie na magazyn")
                    .Where(e => e.Ilosc != null && e.Wydano != null)
                    .OrderByDescending(e => e.Ilosc - e.Wydano)
                    .Where(e => e.Ilosc - e.Wydano > 0)
                    .FirstOrDefault();

                if(przyjecieElementRuchu == null)
                {
                    logger.LogWarning($"Nie można zdjąć zerowego stanu towaru {towar.KodKreskowy}. Zamówienie: {order.GetUrl()}");

                    continue;
                }

                przyjecieElementRuchu.Wydano++;

                var zaleznoscPZWZ = new ZaleznosciPZWZ
                {
                    ElementPZ = przyjecieElementRuchu,
                    ElementWZ = wydanieElementRuchu,
                    Ilosc = 1
                };

                lomagDbContext.RuchyMagazynowe.Add(ruchMagazynowy);
                lomagDbContext.ElementyRuchuMagazynowego.Add(wydanieElementRuchu);
                lomagDbContext.ZaleznosciPZWZ.Add(zaleznoscPZWZ);
            }

            wmprojackDbContext.AddRange(ordersToSync);
        }
    }
}
