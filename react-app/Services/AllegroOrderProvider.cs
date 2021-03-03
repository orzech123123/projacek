using Microsoft.AspNetCore.Hosting;
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
}
