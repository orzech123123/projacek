using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using react_app.Configuration;
using react_app.Services;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace react_app.Apilo
{
    public class ApiloOrderProvider : IOrderProvider
    {
        private readonly IOptions<ApiloSettings> apiloSettings;
        private readonly IOptions<Settings> settings;

        public OrderProviderType Type => OrderProviderType.Apilo;

        public ApiloOrderProvider(IOptions<ApiloSettings> apaczkaSettings,
            IOptions<Settings> settings)
        {
            this.apiloSettings = apaczkaSettings;
            this.settings = settings;
        }

        public IEnumerable<OrderDto> GetOrders()
        {
            yield break;
            //var expires = string.Join("", DateTime.Now.AddMinutes(10).ToString("o").Take(19));

            //var expiresDate = new DateTimeOffset(DateTime.Parse(expires)).ToUnixTimeSeconds(); ;

            //var data = new
            //{
            //    page = 1,
            //    limit = 25
            //};
            //var dataJson = JsonConvert.SerializeObject(data);

            //var route = "orders/";

            //var message = $"{apiloSettings.Value.AppId}:{route}:{dataJson}:{expiresDate}";
            //var signature = HashHmac(message, apiloSettings.Value.SecretId);

            //var client = new RestClient("https://www.apaczka.pl");
            //var request = new RestRequest($"api/v2/{route}", Method.POST);

            //request.AddParameter("app_id", apiloSettings.Value.AppId);
            //request.AddParameter("request", dataJson);
            //request.AddParameter("expires", expiresDate);
            //request.AddParameter("signature", signature);

            //var response = client.Execute<ApaczkaOrdersResponse>(request).Data;

            //var orders = response.Response.Orders.Where(o => o.Created > settings.Value.StartOrdersSyncFrom);

            //return orders
            //    .Select(o => new OrderDto
            //    {
            //        ProviderOrderId = o.Id,
            //        ProviderType = OrderProviderType.Apaczka,
            //        Codes = o.Comment,
            //        Name = o.Content,
            //        //Date = o.Created,
            //        Quantity = 1
            //    });
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

        public string GenerateUrl(string orderId)
        {
            return $"https://panel.apaczka.pl/zlecenia/{orderId}";
        }
    }
}
