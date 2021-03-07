using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using react_app.Allegro;
using react_app.Wmprojack.Entities;
using RestSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace react_app.Services
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

        public IEnumerable<OrderDto> GetOrders()
        {
            if (!File.Exists(Path.Combine(env.ContentRootPath, "token")))
            {
                logger.LogError($"Problem z połączeniem do Allegro. Brak tokenu. Zaloguj się");

                yield break;
            }

            var token = File.ReadAllText(Path.Combine(env.ContentRootPath, "token"));

            var client2 = new RestClient("https://api.allegro.pl");
            var request2 = new RestRequest($"/order/checkout-forms", Method.GET);
            request2.AddHeader("Authorization", $"Bearer {token}");
            request2.AddHeader("Accept", $"application/vnd.allegro.public.v1+json");
            request2.AddParameter("limit", "25");
            request2.AddParameter("fulfillment.status", "SENT");
            request2.AddParameter("updatedAt.gte", settings.Value.StartOrdersSyncFrom.AddHours(-1).ToString("yyyy-MM-ddTHH:mm:ssZ"));

            var response = client2.Execute<AllegroCheckoutFormsResponse>(request2).Data;

            var offers = response.CheckoutForms.SelectMany(f => f.LineItems.Select(li => new
            {
                ProviderOrderId = f.Id,
                f.UpdatedAt,
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
                    Date = offer.UpdatedAt.AddHours(1),
                    Quantity = offer.Quantity
                };
            }
        }
    }
}
