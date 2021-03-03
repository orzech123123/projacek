using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using react_app.Allegro;
using react_app.Apaczka;
using react_app.Wmprojack.Entities;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace react_app.Services
{
    public interface IOrderProvider
    {
        IEnumerable<OrderDto> GetOrders();
    }

    public class AllegroOrderProvider : IOrderProvider
    {
        private readonly IWebHostEnvironment env;

        public AllegroOrderProvider(IWebHostEnvironment env)
        {
            this.env = env;
        }

        public IEnumerable<OrderDto> GetOrders()
        {
            var token = File.ReadAllText(Path.Combine(env.ContentRootPath, "token"));

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

                yield return new OrderDto
                {
                    ProviderOrderId = offer.ProviderOrderId,
                    ProviderType = OrderProvider.Allegro,
                    Name = saleOffer.Name,
                    Codes = saleOffer.External?.Id,
                    Date = offer.BoughtAt.AddHours(1),
                    Quantity = offer.Quantity
                };
            }
        }
    }

    public class ApaczkaOrderProvider : IOrderProvider
    {
        private readonly IOptions<ApaczkaSettings> apaczkaSettings;

        public ApaczkaOrderProvider(IOptions<ApaczkaSettings> apaczkaSettings)
        {
            this.apaczkaSettings = apaczkaSettings;
        }

        public IEnumerable<OrderDto> GetOrders()
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

            var response = client.Execute<ApaczkaOrdersResponse>(request).Data;

            return response.Response.Orders
                .Select(o => new OrderDto
                {
                    ProviderOrderId = o.Id,
                    ProviderType = OrderProvider.Apaczka,
                    Codes = o.Comment,
                    Name = o.Content,
                    Date = o.Created,
                    Quantity = 1
                });
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
