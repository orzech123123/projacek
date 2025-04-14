using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace react_app.Apilo
{
    public class ApiloOrderProvider : IOrderProvider
    {
        private int fetchHowFarInPastInDays = 14;

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

            var minimumFetchDate = DateTime.Now.AddDays(-fetchHowFarInPastInDays).Date;
            var orderedAfter = new[] { settings.Value.StartOrdersSyncFrom, minimumFetchDate }.Max();

            request.AddHeader("Authorization", $"Bearer {accessToken}");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.AddQueryParameter("orderedAfter", orderedAfter.ToString("yyyy-MM-ddTHH:mm:sszzz"));

            var result = new List<OrderDto>();

            var response = await client.ExecuteAsync<ApiloOrdersResponse>(request);

            if (response.IsSuccessful && response.Data != null && response.Data.Orders != null)
            {
                foreach (var apiloOrder in response.Data.Orders)
                {
                    var orderDetailRequest = new RestRequest($"/rest/api/orders/{apiloOrder.Id}/", Method.Get);
                    orderDetailRequest.AddHeader("Authorization", $"Bearer {accessToken}");
                    orderDetailRequest.AddHeader("Content-Type", "application/json");
                    orderDetailRequest.AddHeader("Accept", "application/json");

                    var orderDetailResponse = await client.ExecuteAsync<ApiloOrderDetailResponse>(orderDetailRequest);

                    if (orderDetailResponse.IsSuccessful && orderDetailResponse.Data?.OrderNotes?.FirstOrDefault() != null)
                    {
                        var wiadomoscWewnetrznaType = 2;

                        var comments = orderDetailResponse.Data.OrderNotes
                            .Where(x => x.Type == wiadomoscWewnetrznaType)
                            .Select(x => x.Comment)
                            .ToList();

                        foreach(var comment in comments)
                        {
                            if (!string.IsNullOrEmpty(comment))
                            {
                                var codes = GetCommentCodes(comment);

                                result.Add(new OrderDto
                                {
                                    Codes = string.Join(" ", codes).Trim(),
                                    ProviderOrderId = apiloOrder.Id,
                                    ProviderType = OrderProviderType.Apilo,
                                    Quantity = 1
                                });

                                //logger.LogInformation(comment);
                            }
                        }
                    }
                }

            }
            else
            {
                logger.LogError($"Błąd podczas pobierania zamówień z Apilo: {response.ErrorMessage}, Status Code: {response.StatusCode}, Content: {response.Content}");
            }

            return result;
        }

        private List<string> GetCommentCodes(string comment)
        {
            if (string.IsNullOrEmpty(comment))
            {
                return new List<string>();
            }

            var contents = new List<string>();
            var matches = Regex.Matches(comment, @"<([a-zA-Z0-9]+)>(.*?)</\1>", RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                if (match.Groups.Count == 3) 
                {
                    contents.Add(match.Groups[2].Value); 
                }
            }
            return contents;
        }

        public string GenerateUrl(string orderId)
        {
            return $"https://wmprojack.apilo.com/order/order/detail/{orderId}";
        }
    }
}
