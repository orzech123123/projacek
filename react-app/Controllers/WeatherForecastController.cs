using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace react_app.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Order> Get()
        {
            //var rng = new Random();
            //return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            //{
            //    Date = DateTime.Now.AddDays(index),
            //    TemperatureC = rng.Next(-20, 55),
            //    Summary = Summaries[rng.Next(Summaries.Length)]
            //})
            //.ToArray();

            var expires = string.Join("", DateTime.Now.AddMinutes(10).ToString("o").Take(19));
            //var expires = "2021-02-23T15:25:00";
            var expiresDate = new DateTimeOffset(DateTime.Parse(expires)).ToUnixTimeSeconds(); ;

            var data = new
            {
                page = 1,
                limit = 10
            };
            var dataJson = JsonConvert.SerializeObject(data);

            var appId = "1248335_6030e705195b38.51428106";
            var appSecret = "3rrm587t6hx4buctdj8xf9hsnju455f6";

            var route = "orders/";

            var message = $"{appId}:{route}:{dataJson}:{expiresDate}";
            var signature = HashHmac(message, appSecret);



            var client = new RestClient("https://www.apaczka.pl");
            var request = new RestRequest($"api/v2/{route}", Method.POST);

            request.AddParameter("app_id", appId);
            request.AddParameter("request", dataJson);
            request.AddParameter("expires", expiresDate);
            request.AddParameter("signature", signature);

            var queryResult = client.Execute<OrdersResponse>(request);

            return queryResult.Data.Response.Orders;
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
