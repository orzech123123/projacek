using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using react_app.Configuration;
using react_app.Services;
using RestSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace react_app.Allegro
{
    public class AllegroOrderProvider : IOrderProvider
    {
        private readonly IWebHostEnvironment env;
        private readonly IOptions<Settings> settings;
        private readonly ILogger<AllegroOrderProvider> logger;

        public AllegroOrderProvider(IWebHostEnvironment env, IOptions<Settings> settings, ILogger<AllegroOrderProvider> logger)
        {
            this.env = env;
            this.settings = settings;
            this.logger = logger;
        }

        public OrderProviderType Type => OrderProviderType.Allegro;

        public string GenerateUrl(string orderId)
        {
            return $"https://allegro.pl/moje-allegro/sprzedaz/zamowienia/{orderId}";
        }

        public IEnumerable<OrderDto> GetOrders()
        {
            if (!File.Exists(Path.Combine(env.ContentRootPath, "token")))
            {
                logger.LogError($"Problem z połączeniem do Allegro. Brak tokenu. Zaloguj się");

                return Enumerable.Empty<OrderDto>();
            }

            var token = File.ReadAllText(Path.Combine(env.ContentRootPath, "token"));

            var client = new RestClient("https://api.allegro.pl");
            var request = new RestRequest($"/order/checkout-forms", Method.GET);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Accept", $"application/vnd.allegro.public.v1+json");
            //request.AddParameter("limit", "25");
            request.AddParameter("fulfillment.status", "SENT");
            request.AddParameter("updatedAt.gte", settings.Value.StartOrdersSyncFrom.AddHours(-1).ToString("yyyy-MM-ddTHH:mm:ssZ"));

            var response = client.Execute<AllegroCheckoutFormsResponse>(request).Data;

            var orders = response.CheckoutForms.SelectMany(f => f.LineItems.Select(li => new
            {
                ProviderOrderId = f.Id,
                f.UpdatedAt,
                OfferId = li.Offer.Id,
                li.Quantity
            }));

            if(!orders.Any())
            {
                return Enumerable.Empty<OrderDto>();
            }

            request = new RestRequest($"sale/offers", Method.GET);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddParameter("limit", "1000");
            request.AddHeader("Accept", $"application/vnd.allegro.public.v1+json");

            var offers = client.Execute<AllegroSaleOfferResponse>(request)
                .Data.Offers
                .ToDictionary(o => o.Id);

            orders = orders.Where(order => offers.ContainsKey(order.OfferId));

            return orders.Select(order => new OrderDto
            {
                ProviderOrderId = order.ProviderOrderId,
                ProviderType = OrderProviderType.Allegro,
                Name = offers[order.OfferId].Name,
                Codes = offers[order.OfferId].External?.Id,
                //Date = offer.UpdatedAt.AddHours(1),
                Quantity = order.Quantity
            });
        }
    }
}
