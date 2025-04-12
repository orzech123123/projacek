//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using react_app.Configuration;
//using react_app.Services;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;

//namespace react_app.Allegro
//{
//    public class AllegroOrderProvider : IOrderProvider
//    {
//        private readonly IWebHostEnvironment env;
//        private readonly IOptions<Settings> settings;
//        private readonly ILogger<AllegroOrderProvider> logger;
//        private readonly AllegroOfferService allegroOfferService;
//        private readonly AllegroOrderService allegroOrderService;

//        public AllegroOrderProvider(
//            IWebHostEnvironment env,
//            IOptions<Settings> settings,
//            ILogger<AllegroOrderProvider> logger,
//            AllegroOfferService allegroOfferService,
//            AllegroOrderService allegroOrderService)
//        {
//            this.env = env;
//            this.settings = settings;
//            this.logger = logger;
//            this.allegroOfferService = allegroOfferService;
//            this.allegroOrderService = allegroOrderService;
//        }

//        public OrderProviderType Type => OrderProviderType.Allegro;

//        public string GenerateUrl(string orderId)
//        {
//            return $"https://allegro.pl/moje-allegro/sprzedaz/zamowienia/{orderId}";
//        }

//        public IEnumerable<OrderDto> GetOrders()
//        {
//            if (!File.Exists(Path.Combine(env.ContentRootPath, "token")))
//            {
//                logger.LogError($"Problem z połączeniem do Allegro. Brak tokenu. Zaloguj się");

//                return Enumerable.Empty<OrderDto>();
//            }

//            var orders = allegroOrderService
//                .GetAll()
//                .SelectMany(f => f.LineItems.Select(li => new
//                {
//                    ProviderOrderId = f.Id,
//                    f.UpdatedAt,
//                    OfferId = li.Offer.Id,
//                    li.Quantity
//                }));

//            if(!orders.Any())
//            {
//                return Enumerable.Empty<OrderDto>();
//            }

//            var offers = allegroOfferService.GetAll();

//            orders = orders.Where(order => offers.ContainsKey(order.OfferId));

//            return orders.Select(order => new OrderDto
//            {
//                ProviderOrderId = order.ProviderOrderId,
//                ProviderType = OrderProviderType.Allegro,
//                Name = offers[order.OfferId].Name,
//                Codes = offers[order.OfferId].Signature,
//                //Date = offer.UpdatedAt.AddHours(1),
//                Quantity = order.Quantity
//            });
//        }
//    }
//}
