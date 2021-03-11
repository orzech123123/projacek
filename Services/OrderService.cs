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
        private readonly WmprojackDbContext wmprojackDbContext;
        private readonly LomagDbContext lomagDbContext;
        private readonly ILogger<OrderService> logger;
        private readonly LomagService lomagService;

        public OrderService(
            IEnumerable<IOrderProvider> orderProviders,
            WmprojackDbContext wmprojackDbContext,
            LomagDbContext lomagDbContext,
            ILogger<OrderService> logger,
            LomagService lomagService)
        {
            this.orderProviders = orderProviders;
            this.wmprojackDbContext = wmprojackDbContext;
            this.lomagDbContext = lomagDbContext;
            this.logger = logger;
            this.lomagService = lomagService;
        }

        public async Task<int> SyncOrdersAsync()
        {
            var lomagTowars = await lomagService.GetTowary();
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

            var ordersToSync = GetOrdersToSync(recentApiOrders, lomagTowars);
            logger.LogInformation($"Liczba wydań towarów do synchronizacji: {ordersToSync.Count()}");

            if (!ordersToSync.Any())
            {
                return 0;
            }

            var addedOrders = AddOrdersToDbs(ordersToSync, lomagTowars).ToList();

            wmprojackDbContext.SaveChanges();
            //lomagDbContext.SaveChanges();

            return addedOrders.Count();
        }

        private IList<Order> GetOrdersToSync(IEnumerable<OrderDto> recentApiOrders, IEnumerable<Towar> lomagTowars)
        {
            var ordersToSync = new List<Order>();

            var syncedOrders = wmprojackDbContext.Orders.ToList();
            var lomagKodyKreskowe = lomagTowars.Select(t => t.KodKreskowy);

            var apiOrdersGroups = recentApiOrders
                .Where(o => o.IsValid)
                .GroupBy(o => new { o.ProviderOrderId, o.ProviderType, o.Name, o.Date });

            foreach (var apiOrderGroup in apiOrdersGroups)
            {
                var missingCodes = apiOrderGroup
                    .SelectMany(g => GetCodes(g, lomagKodyKreskowe))
                    .ToList();

                var syncedOrdersForOrderGroup = syncedOrders
                    .Where(o => o.ProviderOrderId == apiOrderGroup.Key.ProviderOrderId)
                    .Where(o => o.ProviderType == apiOrderGroup.Key.ProviderType);

                foreach(var syncedOrder in syncedOrdersForOrderGroup)
                {
                    var firstCodeMatch = missingCodes.FirstOrDefault(c => c == syncedOrder.Code);
                    if(firstCodeMatch != null)
                    {
                        missingCodes.Remove(firstCodeMatch);
                    }
                }

                foreach(var missingCode in missingCodes)
                {
                    var name = $"{lomagTowars.Single(t => t.KodKreskowy == missingCode).Nazwa} - {apiOrderGroup.Key.Name}";

                    ordersToSync.Add(new Order
                    {
                        Code = missingCode,
                        Date = apiOrderGroup.Key.Date,
                        Name = name,
                        ProviderOrderId = apiOrderGroup.Key.ProviderOrderId,
                        ProviderType = apiOrderGroup.Key.ProviderType
                    });
                }
            }

            return ordersToSync;
        }

        private IEnumerable<string> GetCodes(OrderDto apiOrder, IEnumerable<string> lomagKodyKreskowe)
        {
            var codes = apiOrder.Codes
                .Split(new[] { ' ' })
                .Where(strPart => lomagKodyKreskowe.Contains(strPart))
                .ToList();

            var allCodes = new List<string>();

            for(var i = 0; i < apiOrder.Quantity; i++){
                allCodes.AddRange(codes);
            }

            return allCodes;
        }

        private IEnumerable<Order> AddOrdersToDbs(IEnumerable<Order> ordersToSync, IEnumerable<Towar> lomagTowars)
        {
            var allegroKontrahent = lomagService.GetAllegroKontrahent();
            var wmprojackKontrahent = lomagService.GetWmProjackKontrahent();
            var wmprojackMagazyn = lomagService.GetProjackMagazyn();
            var wydanieRodzajRuchu = lomagService.GetWydanieZMagazynuRodzajRuchu();
            var user = lomagService.GetUzytkownik();

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

                var przyjecieElementRuchu = lomagService.GetWolnePrzyjecia(towar)[towar.IdTowaru]?.FirstOrDefault();
                if (przyjecieElementRuchu == null)
                {
                    logger.LogWarning($"Nie można zdjąć zerowego stanu towaru {towar.KodKreskowy}. Zamówienie: {order.GetUrl()}");
                    continue;
                }

                var wydanieElementRuchu = new ElementRuchuMagazynowego
                {
                    Towar = towar,
                    CenaJednostkowa = 0,
                    Ilosc = 1,
                    RuchMagazynowy = ruchMagazynowy,
                    Utworzono = date,
                    Zmodyfikowano = date,
                    Uzytkownik = user,
                    Uwagi = order.GetUrl()
                };

                przyjecieElementRuchu.Wydano = przyjecieElementRuchu.Wydano != null ? przyjecieElementRuchu.Wydano + 1 : 1;

                var zaleznoscPZWZ = new ZaleznosciPZWZ
                {
                    ElementPZ = przyjecieElementRuchu,
                    ElementWZ = wydanieElementRuchu,
                    Ilosc = 1
                };

                lomagDbContext.RuchyMagazynowe.Add(ruchMagazynowy);
                lomagDbContext.ElementyRuchuMagazynowego.Add(wydanieElementRuchu);
                lomagDbContext.ZaleznosciPZWZ.Add(zaleznoscPZWZ);

                wmprojackDbContext.Add(order);
                yield return order;
            }
        }
    }
}
