﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using react_app.Configuration;
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
        private readonly IOptions<Settings> settings;

        public OrderService(
            IEnumerable<IOrderProvider> orderProviders,
            WmprojackDbContext wmprojackDbContext,
            LomagDbContext lomagDbContext,
            ILogger<OrderService> logger,
            LomagService lomagService,
            IOptions<Settings> settings)
        {
            this.orderProviders = orderProviders;
            this.wmprojackDbContext = wmprojackDbContext;
            this.lomagDbContext = lomagDbContext;
            this.logger = logger;
            this.lomagService = lomagService;
            this.settings = settings;
        }

        public async Task<int> SyncOrdersAsync()
        {
            var towary = await lomagService.GetTowary();

            var allOrders = new List<OrderDto>();
            foreach (var provider in orderProviders)
            {
                var ordersFromProvider = await provider.GetOrders();
                allOrders.AddRange(ordersFromProvider);
            }

            /////////////// testy jedno zamowienie /////////////////////////////////////////
            //allOrders = allOrders.Where(o => o.ProviderOrderId == "CG250400304").ToList();

            var recentApiOrders = allOrders.ToList();

            var ordersToSync = GetOrdersToSync(recentApiOrders, towary);
            logger.LogInformation($"Liczba wydań towarów do synchronizacji: {ordersToSync.Count()}");

            if (!ordersToSync.Any())
            {
                return 0;
            }

            var addedOrders = AddOrdersToDbs(ordersToSync, towary).ToList();

            if(addedOrders.Any())
            {
                wmprojackDbContext.SaveChanges();
                lomagDbContext.SaveChanges();
            }

            return addedOrders.Count();
        }

        private IList<Order> GetOrdersToSync(IEnumerable<OrderDto> recentApiOrders, IEnumerable<Towar> towary)
        {
            var ordersToSync = new List<Order>();

            var alreadyProcessedOrders = wmprojackDbContext.Orders
                .Select(o => new { o.ProviderOrderId, o.ProviderType, o.Code })
                .ToList()
                .Concat(wmprojackDbContext.IgnoredOrder
                .Select(o => new { o.ProviderOrderId, o.ProviderType, o.Code })
                .ToList());

            var lomagKodyKreskowe = towary.Select(t => t.KodKreskowy);

            var apiOrdersGroups = recentApiOrders
                .Where(o => o.IsValid)
                .GroupBy(o => new { o.ProviderOrderId, o.ProviderType/*, o.Name*/ });

            foreach (var apiOrderGroup in apiOrdersGroups)
            {
                var missingCodes = apiOrderGroup
                    .SelectMany(g =>
                    {
                        var lomagMatchedCodes = lomagService.ExtractCodes(g.Codes, lomagKodyKreskowe, g.Quantity, (string invalidCode) =>
                        {
                            LogNiepoprawnyKod(invalidCode, apiOrderGroup.Key.ProviderOrderId, apiOrderGroup.Key.ProviderType);
                        });

                        return lomagMatchedCodes;
                    })
                    .ToList();

                var alreadyProcessedOrdersGroup = alreadyProcessedOrders
                    .Where(o => o.ProviderOrderId == apiOrderGroup.Key.ProviderOrderId)
                    .Where(o => o.ProviderType == apiOrderGroup.Key.ProviderType);

                foreach(var processedOrder in alreadyProcessedOrdersGroup)
                {
                    var firstCodeMatch = missingCodes.FirstOrDefault(c => c == processedOrder.Code);
                    if(firstCodeMatch != null)
                    {
                        missingCodes.Remove(firstCodeMatch);
                    }
                }

                foreach(var missingCode in missingCodes)
                {
                    //var name = $"{towary.Single(t => t.KodKreskowy == missingCode).Nazwa} - {apiOrderGroup.Key.Name}";

                    var towar = towary.Single(t => t.KodKreskowy == missingCode);

                    var name = $"{towar.Nazwa}"; 

                    ordersToSync.Add(new Order
                    {
                        Code = missingCode,
                        Date = DateTime.Now,//apiOrderGroup.Key.Date,
                        Name = name,
                        ProviderOrderId = apiOrderGroup.Key.ProviderOrderId,
                        ProviderType = apiOrderGroup.Key.ProviderType
                    });
                }
            }

            return ordersToSync;
        }

        private IEnumerable<Order> AddOrdersToDbs(IEnumerable<Order> ordersToSync, IEnumerable<Towar> towary)
        {
            var bezWolnychPrzyjec = GetBezWolnychPrzyjec(towary, ordersToSync);
            if (bezWolnychPrzyjec.Count() == ordersToSync.Count())
            {
                LogBrakStanu(towary, bezWolnychPrzyjec);
                yield break;
            }

            var apiloKontrahent = lomagService.GetApiloKontrahent();
            var wmprojackKontrahent = lomagService.GetWmProjackKontrahent();
            var wmprojackMagazyn = lomagService.GetMagazyn2022();
            var wydanieRodzajRuchu = lomagService.GetWydanieZMagazynuRodzajRuchu();
            var user = lomagService.GetUzytkownik();

            foreach (var order in ordersToSync)
            {
                var date = order.Date; 

                var ruchMagazynowy = new RuchMagazynowy
                {
                    Kontrahent = apiloKontrahent,
                    Company = wmprojackKontrahent,
                    Data = date,
                    Utworzono = date,
                    Zmodyfikowano = date,
                    Magazyn = wmprojackMagazyn,
                    NrDokumentu = $"WZ/{order.ProviderOrderId}/{order.Code}",
                    RodzajRuchuMagazynowego = wydanieRodzajRuchu,
                    Uzytkownik = user,
                    Operator = -1
                };

                var towar = towary.Single(t => t.KodKreskowy == order.Code);

                var przyjecieElementRuchu = lomagService.GetWolnePrzyjecia(t => t.TowarId, towar)[towar.IdTowaru]?.FirstOrDefault();
                if (przyjecieElementRuchu == null)
                {
                    LogBrakStanu(towar, order);
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
                    Uwagi = GenerateUrl(order)
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


        private IEnumerable<Order> GetBezWolnychPrzyjec(IEnumerable<Towar> towary, IEnumerable<Order> ordersToSync)
        {
            var przyjecia = lomagService.GetWolnePrzyjecia(t => t.TowarId);

            foreach(var order in ordersToSync)
            {
                var towar = towary.Single(t => t.KodKreskowy == order.Code);
                var przyjecieElementRuchu = przyjecia[towar.IdTowaru]?.FirstOrDefault();

                if (przyjecieElementRuchu == null)
                {
                    yield return order;
                }
            }
        }

        private void LogBrakStanu(IEnumerable<Towar> towary, IEnumerable<Order> bezWolnychPrzyjec)
        {
            foreach (var order in bezWolnychPrzyjec)
            {
                var towar = towary.Single(t => t.KodKreskowy == order.Code);
                LogBrakStanu(towar, order);
            }
        }

        private void LogBrakStanu(Towar towar, Order order)
        {
            logger.LogWarning($"Nie można zdjąć zerowego stanu towaru {towar.KodKreskowy}." +
                $" [Zamówienie: {GenerateUrl(order)}] [STOP: {settings.Value.StopSyncOrdersUrl}?id={order.ProviderOrderId}&code={order.Code}&type={order.ProviderType.ToString()}]");
        }

        private void LogNiepoprawnyKod(string code, string orderId, OrderProviderType providerType)
        {
            logger.LogWarning($"Błędny kod towaru {code}." +
                $" [Zamówienie: {GenerateUrl(orderId, providerType)}]");
        }

        private string GenerateUrl(Order order) => orderProviders.Single(p => p.Type == order.ProviderType).GenerateUrl(order.ProviderOrderId);
        private string GenerateUrl(string orderId, OrderProviderType providerType) => orderProviders.Single(p => p.Type == providerType).GenerateUrl(orderId);
    }
}
