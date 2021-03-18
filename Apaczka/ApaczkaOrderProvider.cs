using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using react_app.Configuration;
using react_app.Services;
using react_app.Wmprojack.Entities;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace react_app.Apaczka
{
    public class ApaczkaOrderProvider : IOrderProvider
    {
        private readonly IOptions<ApaczkaSettings> apaczkaSettings;
        private readonly IOptions<Settings> settings;

        public ApaczkaOrderProvider(IOptions<ApaczkaSettings> apaczkaSettings,
            IOptions<Settings> settings)
        {
            this.apaczkaSettings = apaczkaSettings;
            this.settings = settings;
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

            var orders = response.Response.Orders.Where(o => o.Created > settings.Value.StartOrdersSyncFrom);

            return orders
                .Select(o => new OrderDto
                {
                    ProviderOrderId = o.Id,
                    ProviderType = OrderProvider.Apaczka,
                    Codes = o.Comment,
                    Name = o.Content,
                    //Date = o.Created,
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
