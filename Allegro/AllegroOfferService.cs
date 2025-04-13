//using Microsoft.AspNetCore.Hosting;
//using Newtonsoft.Json.Serialization;
//using react_app.Converters;
//using RestSharp;
//using RestSharp.Serializers.NewtonsoftJson;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;

//namespace react_app.Allegro
//{
//    public class AllegroOfferService
//    {
//        private readonly RestClient client;
//        private readonly IWebHostEnvironment env;

//        public AllegroOfferService(IWebHostEnvironment env)
//        {
//            this.env = env;
//            client = new RestClient("https://api.allegro.pl");
//        }

//        private string GetToken() => File.ReadAllText(Path.Combine(env.ContentRootPath, "token"));

//        public IDictionary<string, AllegroSaleOffer> GetAll()
//        {
//            var request = new RestRequest($"sale/offers", Method.GET);
//            request.AddHeader("Authorization", $"Bearer {GetToken()}");
//            request.AddHeader("Accept", $"application/vnd.allegro.public.v1+json");
//            request.AddParameter("limit", "1000");

//            var offers = client.Execute<AllegroSaleOffers>(request)
//                .Data.Offers
//                .ToDictionary(o => o.Id);

//            return offers;
//        }

//        public AllegroSaleOffer GetById(string offerId)
//        {
//            var request = new RestRequest($"sale/offers/{offerId}", Method.Get);
//            request.AddHeader("Authorization", $"Bearer {GetToken()}");
//            request.AddHeader("Accept", $"application/vnd.allegro.public.v1+json");

//            var response = client.Execute<AllegroSaleOffer>(request);

//            return response.Data;
//        }

//        public AllegroSaleOffer Update(AllegroSaleOffer offer, int available)
//        {
//            var request = new RestRequest($"sale/offers/{offer.Id}", Method.Put);
//            client.UseNewtonsoftJson(new Newtonsoft.Json.JsonSerializerSettings {
//                ContractResolver = new CamelCasePropertyNamesContractResolver(),
//                Converters = new [] { new UpperCaseStringEnumConverter() }
//            });
//            request.AddHeader("Authorization", $"Bearer {GetToken()}");
//            request.AddHeader("Accept", $"application/vnd.allegro.public.v1+json");
//            request.AddHeader("Content-Type", $"application/vnd.allegro.public.v1+json");

//            offer = GetById(offer.Id);

//            offer.Stock.Available = available;

//            request.AddJsonBody(offer);

//            var response = client.Execute<AllegroOfferCommand>(request);
//            if (!response.IsSuccessful)
//            {
//                throw new InvalidOperationException(nameof(Update));
//            }

//            return offer;
//        }

//        public AllegroSaleOffer Activate(AllegroSaleOffer offer)
//        {
//            var request = new RestRequest($"sale/offer-publication-commands/{Guid.NewGuid()}", Method.PUT);
//            request.AddHeader("Authorization", $"Bearer {GetToken()}");
//            request.AddHeader("Accept", $"application/vnd.allegro.public.v1+json");
//            request.AddHeader("Content-Type", $"application/vnd.allegro.public.v1+json");
//            request.AddJsonBody(new
//            {
//                offerCriteria = new[]
//                {
//                    new
//                    {
//                        type = "CONTAINS_OFFERS",
//                        offers = new[]
//                        {
//                            new { id = offer.Id }
//                        }
//                    }
//                },
//                publication = new
//                {
//                    action = "ACTIVATE"
//                }
//            });

//            var response = client.Execute<AllegroOfferCommand>(request);
//            if(!response.IsSuccessful)
//            {
//                throw new InvalidOperationException(nameof(Activate));
//            }

//            return offer;
//        }
//    }
//}
