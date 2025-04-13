using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using react_app.Configuration;
using RestSharp;
using System.Collections.Generic;
using System.IO;

namespace react_app.Allegro
{
    public class AllegroOrderService
    {
        private readonly IWebHostEnvironment env;
        private readonly IOptions<Settings> settings;

        public AllegroOrderService(
            IWebHostEnvironment env,
            IOptions<Settings> settings)
        {
            this.env = env;
            this.settings = settings;
        }

        public IEnumerable<AllegroCheckoutForm> GetAll()
        {
            var token = File.ReadAllText(Path.Combine(env.ContentRootPath, "token"));

            var client = new RestClient("https://api.allegro.pl");

            var request = new RestRequest($"/order/checkout-forms", Method.Get);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Accept", $"application/vnd.allegro.public.v1+json");
            //request.AddParameter("limit", "25");
            request.AddParameter("fulfillment.status", "SENT");
            request.AddParameter("updatedAt.gte", settings.Value.StartOrdersSyncFrom.AddHours(-1).ToString("yyyy-MM-ddTHH:mm:ssZ"));

            var response = client.Execute<AllegroCheckoutForms>(request).Data;

            var orders = response.CheckoutForms;

            return orders;
        }
    }
}
