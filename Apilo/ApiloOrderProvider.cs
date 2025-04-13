using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using react_app.BackgroundTasks;
using react_app.Configuration;
using react_app.Services;
using react_app.Wmprojack.Entities;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace react_app.Apilo
{
    public class ApiloOrderProvider : IOrderProvider
    {
        private readonly ILogger<ApiloOrderProvider> logger;
        private readonly IOptions<ApiloSettings> apiloSettings;
        private readonly IOptions<Settings> settings;
        private readonly IWebHostEnvironment env;

        public OrderProviderType Type => OrderProviderType.Apilo;

        public ApiloOrderProvider(
            ILogger<ApiloOrderProvider> logger,
            IOptions<ApiloSettings> apiloSettings,
             IOptions<Settings> settings,
             IWebHostEnvironment env) 
        {
            this.logger = logger;
            this.apiloSettings = apiloSettings;
            this.settings = settings;
            this.env = env;
        }

        public async Task<IEnumerable<OrderDto>> GetOrders()
        {
            var tokenFilePath = Path.Combine(env.ContentRootPath, "token");
            if (!File.Exists(tokenFilePath))
            {
                Console.WriteLine("Brak tokenu, nie moge pobrac zamowien z Apilo");
                return Enumerable.Empty<OrderDto>();
            }

            var accessToken = File.ReadAllText(tokenFilePath);
            var client = new RestClient(apiloSettings.Value.Url);

            var request = new RestRequest($"/rest/api/orders/", Method.Get);

            request.AddHeader("Authorization", $"Bearer {accessToken}");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.AddQueryParameter("orderedAfter", settings.Value.StartOrdersSyncFrom.ToString("yyyy-MM-ddTHH:mm:sszzz"));
            //request.AddQueryParameter("orderStatus", "4"); //jaki to Wyslane && Odbior osobisty bo to sa w Apilo - Zrealizowane

            var response = await client.ExecuteAsync<ApiloOrdersResponse>(request);

            if (response.IsSuccessful && response.Data != null && response.Data.Orders != null)
            {
                var data = response.Data.Orders;


                foreach (var apiloOrder in response.Data.Orders)
                {
                    var orderDetailRequest = new RestRequest($"/rest/api/orders/{apiloOrder.Id}/", Method.Get);
                    orderDetailRequest.AddHeader("Authorization", $"Bearer {accessToken}");
                    orderDetailRequest.AddHeader("Content-Type", "application/json");
                    orderDetailRequest.AddHeader("Accept", "application/json");

                    var orderDetailResponse = await client.ExecuteAsync<ApiloOrderDetailResponse>(orderDetailRequest);

                    if (orderDetailResponse.IsSuccessful && orderDetailResponse.Data?.OrderNotes?.FirstOrDefault() != null)
                    {
                        if(orderDetailResponse.Data.OrderNotes.Any())
                        {
                            var comment = orderDetailResponse.Data.OrderNotes.First().Comment;
                        }
                    }
                }

            }
            else
            {
                logger.LogError($"Błąd podczas pobierania zamówień z Apilo: {response.ErrorMessage}, Status Code: {response.StatusCode}, Content: {response.Content}");
            }

            return Enumerable.Empty<OrderDto>();
        }

        public string GenerateUrl(string orderId)
        {
            return $"https://panel.apaczka.pl/zlecenia/{orderId}";
        }
    }
}
