using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using react_app.Allegro;
using react_app.Apaczka;
using react_app.Lomag;
using react_app.Wmprojack;
using react_app.Wmprojack.Entities;
using RestSharp;

namespace react_app.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOptions<ApaczkaSettings> apaczkaSettings;
        private readonly IWebHostEnvironment env;
        private readonly LomagDbContext lomagDbContext;
        private readonly WmprojackDbContext wmprojackDbContext;

        public OrdersController(
            IOptions<ApaczkaSettings> apaczkaSettings,
            IWebHostEnvironment env,
            LomagDbContext lomagDbContext,
            WmprojackDbContext wmprojackDbContext)
        {
            this.apaczkaSettings = apaczkaSettings;
            this.env = env;
            this.lomagDbContext = lomagDbContext;
            this.wmprojackDbContext = wmprojackDbContext;
        }

        [HttpGet]
        public object Sync()
        {
            var projackDbOrders = wmprojackDbContext.Orders.ToList();
            var recentApiOrders = GetRecentOrdersFromProviders();
            var lomagTowars = lomagDbContext.Towary.ToList();

            //TODO remove / tests
            /*recentApiOrders.Add(new Order
            {
                Date = DateTime.Now,
                Name = "dwie sztuki tego samego",
                ProviderOrderId = "05g82981-22gh-11eb-a3df-37uf7454fbg1-allegrokodsztuczny",
                ProviderType = OrderProvider.Allegro,
                Code = "00000292",
                Quantity = 2
            });
            recentApiOrders.Add(new Order
            {
                Date = DateTime.Now,
                Name = "zamówienie z 4 w sumie sztukami",
                ProviderOrderId = "96666666-apaczkowykodsztuczny",
                ProviderType = OrderProvider.Apaczka,
                Code = "00000006 00000053 xxxxxxx 00000072 00000053",
                Quantity = 1
            });*/

            var ordersToSync = GetOrdersToSync(projackDbOrders, recentApiOrders, lomagTowars);
            wmprojackDbContext.AddRange(ordersToSync);
            wmprojackDbContext.SaveChanges();

            AddOrderSyncsToDb(ordersToSync);

            var allSyncs = wmprojackDbContext.Orders.OrderByDescending(o => o.Date).ThenBy(o => o.ProviderOrderId).ToList();
            return new
            {
                orders = recentApiOrders,
                syncs = allSyncs
            };
        }

        private IList<Order> GetOrdersToSync(List<Order> projackDbOrders, IList<Order> recentApiOrders, List<Lomag.Entities.Towar> lomagTowars)
        {
            var ordersToSync = new List<Order>();
            foreach (var apiOrder in recentApiOrders.Where(o => o.IsValid))
            {
                var isDuplicate = projackDbOrders
                    .Union(ordersToSync)
                    .Any(o => o.ProviderOrderId == apiOrder.ProviderOrderId && o.ProviderType == apiOrder.ProviderType);

                if (isDuplicate)
                {
                    continue;
                }

                var lomagKodyKreskowe = lomagTowars.Select(t => t.KodKreskowy);
                var detectedCodes = apiOrder.Code.Split(new[] { ' ' })
                    .Where(strPart => lomagKodyKreskowe.Contains(strPart));

                for (var i = 0; i < apiOrder.Quantity; i++)
                {
                    foreach (var code in detectedCodes)
                    {
                        ordersToSync.Add(new Order
                        {
                            Code = code,
                            Date = apiOrder.Date,
                            Name = $"{lomagTowars.Single(t => t.KodKreskowy == code).Nazwa} - {apiOrder.Name}",
                            ProviderOrderId = apiOrder.ProviderOrderId,
                            ProviderType = apiOrder.ProviderType
                        });
                    }
                }
            }

            return ordersToSync;
        }

        private void AddOrderSyncsToDb(IList<Order> syncs)
        {
            foreach(var order in syncs)
            {
                
            }    

            var yyy = lomagDbContext.ElementyRuchuMagazynowego
                .Include(e => e.Uzytkownik)
                .Include(e => e.RuchMagazynowy)
                .ThenInclude(e => e.RodzajRuchuMagazynowego)
                .ToList();
        }

        private IList<Order> GetRecentOrdersFromProviders()
        {
            var apaczkaOrders = GetApaczkaOrders();
            var allegroOrders = GetAllegroOrders().ToList();

            var orders = apaczkaOrders.Response.Orders
                .Select(o => new Order
                {
                    ProviderOrderId = o.Id,
                    ProviderType = OrderProvider.Apaczka,
                    Code = o.Comment,
                    Name = o.Content,
                    Date = o.Created,
                    Quantity = 1
                })
                .Union(allegroOrders.Select(o => new Order
                {
                    ProviderOrderId = o.OrderId,
                    ProviderType = OrderProvider.Allegro,
                    Name = o.Name,
                    Code = o.External?.Id,
                    Date = o.BoughtAt,
                    Quantity = o.Quantity
                }))
                .OrderByDescending(o => o.Date)
                .ToList();

            return orders;
        }

        private ApaczkaOrdersResponse GetApaczkaOrders()
        {
            var expires = string.Join("", DateTime.Now.AddMinutes(10).ToString("o").Take(19));

            var expiresDate = new DateTimeOffset(DateTime.Parse(expires)).ToUnixTimeSeconds(); ;

            var data = new
            {
                page = 1,
                limit = 25
            };
            var dataJson = JsonConvert.SerializeObject(data);

            var route = "orders/";

            var message = $"{apaczkaSettings.Value.AppId}:{route}:{dataJson}:{expiresDate}";
            var signature = HashHmac(message, apaczkaSettings.Value.SecretId);

            var client = new RestClient("https://www.apaczka.pl");
            var request = new RestRequest($"api/v2/{route}", Method.POST);

            request.AddParameter("app_id", apaczkaSettings.Value.AppId);
            request.AddParameter("request", dataJson);
            request.AddParameter("expires", expiresDate);
            request.AddParameter("signature", signature);

            return client.Execute<ApaczkaOrdersResponse>(request).Data;
        }

        private IEnumerable<AllegroSaleOffer> GetAllegroOrders()
        {
            var token = System.IO.File.ReadAllText(Path.Combine(env.ContentRootPath, "token"));

            var client2 = new RestClient("https://api.allegro.pl");
            var request2 = new RestRequest($"/order/checkout-forms", Method.GET);
            request2.AddHeader("Authorization", $"Bearer {token}");
            request2.AddHeader("Accept", $"application/vnd.allegro.public.v1+json");
            request2.AddParameter("limit", "25");
            //request2.AddParameter("fulfillment.status", "SENT");

            var response = client2.Execute<AllegroCheckoutFormsResponse>(request2).Data;

            var offers = response.CheckoutForms.SelectMany(f => f.LineItems.Select(li => new
            {
                ProviderOrderId = f.Id,
                li.BoughtAt,
                OfferId = li.Offer.Id,
                li.Quantity
            }));

            foreach (var offer in offers)
            {
                var request3 = new RestRequest($"sale/offers/{offer.OfferId}", Method.GET);
                request3.AddHeader("Authorization", $"Bearer {token}");
                request3.AddHeader("Accept", $"application/vnd.allegro.public.v1+json");
                var saleOffer = client2.Execute<AllegroSaleOffer>(request3).Data;

                saleOffer.BoughtAt = offer.BoughtAt;
                saleOffer.OrderId = offer.ProviderOrderId;
                saleOffer.Quantity = offer.Quantity;

                yield return saleOffer;
            }
        }

        private static string HashHmac(string message, string secret)
        {
            Encoding encoding = Encoding.UTF8;
            using (HMACSHA256 hmac = new HMACSHA256(encoding.GetBytes(secret)))
            {
                var msg = encoding.GetBytes(message);
                var hash = hmac.ComputeHash(msg);
                return BitConverter.ToString(hash).ToLower().Replace("-", string.Empty);
            }
        }
    }
}
