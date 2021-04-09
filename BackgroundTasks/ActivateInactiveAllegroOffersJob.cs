using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Microsoft.Extensions.DependencyInjection;
using System;
using react_app.Services;
using react_app.Allegro;
using System.Linq;
using System.Collections.Generic;

namespace react_app.BackgroundTasks
{
    [DisallowConcurrentExecution]
    public class ActivateInactiveAllegroOffersJob : IJob
    {
        private readonly ILogger<ActivateInactiveAllegroOffersJob> _logger;
        private readonly IServiceProvider serviceProvider;
        private readonly AllegroOfferService allegroOfferService;

        private IEnumerable<(string OfferId, IEnumerable<string> Codes, AllegroSaleOfferStatus Status)> offersOnline;

        public ActivateInactiveAllegroOffersJob(
            ILogger<ActivateInactiveAllegroOffersJob> logger,
            IServiceProvider serviceProvider,
            AllegroOfferService allegroOfferService)
        {
            _logger = logger;
            this.serviceProvider = serviceProvider;
            this.allegroOfferService = allegroOfferService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var lomagService = scope.ServiceProvider.GetService<LomagService>();

                var towary = await lomagService.GetTowary();
                var stany = lomagService.GetStany(t => t.Towar.KodKreskowy);
                var offers = allegroOfferService.GetAll();

                var lomagKodyKreskowe = towary.Select(t => t.KodKreskowy);

                var offersWithCodes = offers
                    .Select(o => 
                    (
                        OfferId: o.Key,
                        Codes: lomagService.ExtractCodes(o.Value.Signature, lomagKodyKreskowe, 1),
                        o.Value.Publication.Status
                    ))
                    .Where(o => o.Codes.Any());

                offersOnline = offersWithCodes
                    .Where(o => o.Status != AllegroSaleOfferStatus.Ended)
                    .ToList();

                var offersToTryActivate = offersWithCodes
                    .Where(o => o.Status == AllegroSaleOfferStatus.Ended)
                    .ToList();

                var activatedOfferUrls = offersToTryActivate
                    .Select(o => TryActivateOffer(o, stany))
                    .Where(id => id != null)
                    .Select(id => $"https://allegro.pl/oferta/{id}");

                var activatedOfferUrlsJoined = activatedOfferUrls.Any() ? $"[{string.Join(", ", activatedOfferUrls)}]" : null;

                _logger.LogInformation($"Aktywacja nieaktywnych ofert zakończona powodzeniem. " +
                    $"Aktywowano ofert: {activatedOfferUrls.Count()}\n{activatedOfferUrlsJoined}");
            }
        }

        private string TryActivateOffer(
            (string OfferId, IEnumerable<string> Codes, AllegroSaleOfferStatus Status) offer,
            IDictionary<string, int> stany)
        {
            var canActivate = true; //base on offersOnline and its codes and Stany

            if(canActivate)
            {
                //TODO Activate via API
                offersOnline = offersOnline.Concat(new[] { offer });
            }

            return canActivate ? offer.OfferId : null;
        }
    }
}
