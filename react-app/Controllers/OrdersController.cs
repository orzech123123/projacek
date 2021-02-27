using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using react_app.Allegro;
using react_app.Apaczka;
using react_app.Lomag;
using RestSharp;

namespace react_app.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IOptions<ApaczkaSettings> apaczkaSettings;
        private readonly IWebHostEnvironment env;
        private readonly LomagDbContext lomagDbContext;

        public OrdersController(
            IOptions<ApaczkaSettings> apaczkaSettings,
            IWebHostEnvironment env,
            LomagDbContext lomagDbContext)
        {
            this.apaczkaSettings = apaczkaSettings;
            this.env = env;
            this.lomagDbContext = lomagDbContext;
        }

        [HttpGet]
        public IEnumerable<Order> GetAll()
        {
            var apaczkaOrders = GetApaczkaOrders();
            var allegroOrders = GetAllegroOrders().ToList();

            var towars = lomagDbContext.Towars.Count();

            return apaczkaOrders.Response.Orders.Select(o => new Order
            {
                Type = "apaczka",
                Code = o.Comment,
                Name = o.Content,
                Date = o.Created
            })
            .Union(allegroOrders.Select(o => new Order
            {
                Type = "allegro",
                Name = o.Name,
                Code = o.External?.Id,
                Date = o.BoughtAt
            }))
            .OrderByDescending(o => o.Date);
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
                li.BoughtAt,
                li.Offer.Id,
            }));

            foreach (var offer in offers)
            {
                var request3 = new RestRequest($"sale/offers/{offer.Id}", Method.GET);
                request3.AddHeader("Authorization", $"Bearer {token}");
                request3.AddHeader("Accept", $"application/vnd.allegro.public.v1+json");
                var saleOffer = client2.Execute<AllegroSaleOffer>(request3).Data;
                saleOffer.BoughtAt = offer.BoughtAt;
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
