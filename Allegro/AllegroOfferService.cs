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
        private readonly string token;
        private readonly RestClient client;

        public AllegroOfferService(IWebHostEnvironment env)
        {
            token = File.ReadAllText(Path.Combine(env.ContentRootPath, "token"));
            client = new RestClient("https://api.allegro.pl");
        }

        public IDictionary<string, AllegroSaleOffer> GetAll()
        {
            var request = new RestRequest($"sale/offers", Method.GET);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Accept", $"application/vnd.allegro.public.v1+json");
            request.AddParameter("limit", "1000");

            var offers = client.Execute<AllegroSaleOffers>(request)
                .Data.Offers
                .ToDictionary(o => o.Id);

            return offers;
        }

        public void Update(AllegroSaleOffer offer, int available)
        {
            var request = new RestRequest($"sale/offer-modification-commands/{Guid.NewGuid()}", Method.PUT);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Accept", $"application/vnd.allegro.public.v1+json");
            request.AddHeader("Content-Type", $"application/vnd.allegro.public.v1+json");
            request.AddJsonBody(new
            {
                offerCriteria = new[]
                { 
                    new
                    {
                        type = "CONTAINS_OFFERS",
                        offers = new[]
                        {
                            new { id = offer.Id }
                        }
                    } 
                },
                modification = new
                {
                    /*stock = new
                    {
                        available = available
                    },*/ //TO NIESTETY NIE DZIAŁA I TRZEBA: PUT /sales/offer/{id} NIESTETY TRZEBA PRZESŁAĆ CAŁĄ ENCJĘ, A NIE POLE TYLKO STOCK.AVAILABLE
                    publication = new
                    {
                         durationUnlimited = true
                    }
                }
            });

            var response = client.Execute<AllegroOfferCommand>(request);
        }

        public void Activate(AllegroSaleOffer offer)
        {
            var request = new RestRequest($"sale/offer-publication-commands/{Guid.NewGuid()}", Method.PUT);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddHeader("Accept", $"application/vnd.allegro.public.v1+json");
            request.AddHeader("Content-Type", $"application/vnd.allegro.public.v1+json");
            request.AddJsonBody(new
            {
                offerCriteria = new[]
                {
                    new
                    {
                        type = "CONTAINS_OFFERS",
                        offers = new[]
                        {
                            new { id = offer.Id }
                        }
                    }
                },
                publication = new
                {
                    action = "ACTIVATE"
                }
            });

            var response = client.Execute<AllegroOfferCommand>(request);
        }
    }
}
