﻿using Microsoft.EntityFrameworkCore;
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

            var ordersToSync = GetOrdersToSync(recentApiOrders, lomagTowars);
            logger.LogInformation($"Liczba wydań towarów do synchronizacji: {ordersToSync.Count()}");

            if (!ordersToSync.Any())
            {
                return 0;
            }

            var addedOrders = AddOrdersToDbs(ordersToSync, lomagTowars).ToList();

            if(addedOrders.Any())
            {
                wmprojackDbContext.SaveChanges();
                lomagDbContext.SaveChanges();
            }

            return addedOrders.Count();
        }

        private IList<Order> GetOrdersToSync(IEnumerable<OrderDto> recentApiOrders, IEnumerable<Towar> lomagTowars)
        {
            var ordersToSync = new List<Order>();

            var syncedOrders = wmprojackDbContext.Orders.ToList();
            var lomagKodyKreskowe = lomagTowars.Select(t => t.KodKreskowy);

            var apiOrdersGroups = recentApiOrders
                .Where(o => o.IsValid)
                .GroupBy(o => new { o.ProviderOrderId, o.ProviderType, o.Name });

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
                        Date = DateTime.Now,//apiOrderGroup.Key.Date,
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
            if(AreAllBezWolnychPrzyjec(lomagTowars, ordersToSync))
            {
                yield break;
            }

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
                    NrDokumentu = $"WZ/{order.ProviderOrderId}/{order.Code}",
                    RodzajRuchuMagazynowego = wydanieRodzajRuchu,
                    Uzytkownik = user,
                    Operator = -1
                };

                var towar = lomagTowars.Single(t => t.KodKreskowy == order.Code);

                var przyjecieElementRuchu = lomagService.GetWolnePrzyjecia(towar)[towar.IdTowaru]?.FirstOrDefault();
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

        private bool AreAllBezWolnychPrzyjec(IEnumerable<Towar> lomagTowars, IEnumerable<Order> ordersToSync)
        {
            var przyjecia = lomagService.GetWolnePrzyjecia();
            var areAllBezWolnychPrzyjec = true;

            var brakiToLog = new List<Action>();

            foreach(var order in ordersToSync)
            {
                var towar = lomagTowars.Single(t => t.KodKreskowy == order.Code);
                var przyjecieElementRuchu = przyjecia[towar.IdTowaru]?.FirstOrDefault();

                if (przyjecieElementRuchu == null)
                {
                    brakiToLog.Add(() => LogBrakStanu(towar, order));
                }
                else
                {
                    areAllBezWolnychPrzyjec = false;
                }
            }

            if(areAllBezWolnychPrzyjec)
            {
                brakiToLog.ForEach(b => b());
            }

            return areAllBezWolnychPrzyjec;
        }

        private void LogBrakStanu(Towar towar, Order order)
        {
            logger.LogWarning($"Nie można zdjąć zerowego stanu towaru {towar.KodKreskowy}." +
                $" Zamówienie: {GenerateUrl(order)}");
        }

        private string GenerateUrl(Order order) => orderProviders.Single(p => p.Type == order.ProviderType).GenerateUrl(order.ProviderOrderId);
    }
}