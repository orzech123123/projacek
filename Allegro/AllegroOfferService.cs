using Microsoft.AspNetCore.Hosting;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace react_app.Allegro
{
    public class AllegroOfferService
    {
        private readonly IWebHostEnvironment env;

        public AllegroOfferService(IWebHostEnvironment env)
        {
            this.env = env;
        }

        public IDictionary<string, AllegroSaleOffer> GetAll()
        {
            var token = File.ReadAllText(Path.Combine(env.ContentRootPath, "token"));
            var client = new RestClient("https://api.allegro.pl");

            var request = new RestRequest($"sale/offers", Method.GET);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddParameter("limit", "1000");
            request.AddHeader("Accept", $"application/vnd.allegro.public.v1+json");

            var offers = client.Execute<AllegroSaleOfferResponse>(request)
                .Data.Offers
                .ToDictionary(o => o.Id);

            return offers;
        }
    }
}
