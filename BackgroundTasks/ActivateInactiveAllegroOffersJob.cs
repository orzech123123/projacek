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

        private List<KeyValuePair<string, AllegroSaleOffer>> offersOnline;

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
                var stany = lomagService.GetStany();
                var offers = allegroOfferService.GetAll();

                var lomagKodyKreskowe = towary.Select(t => t.KodKreskowy);

                var offersWithCodes = offers
                    .Where(o => lomagService.ExtractCodes(o.Value.External?.Id, lomagKodyKreskowe, 1).Any());

                offersOnline = offersWithCodes
                    .Where(o => o.Value.Publication.Status != AllegroSaleOfferStatus.Ended)
                    .ToList();

                var offersToTryActivate = offersWithCodes
                    .Where(o => o.Value.Publication.Status == AllegroSaleOfferStatus.Ended)
                    .ToList();

                var activatedOffersCount = offersToTryActivate.Sum(o => TryActivateOffer(o, stany) ? 1 : 0);

                _logger.LogInformation($"Aktywacja nieaktywnych ofert zakończona powidzeniem. Aktywowano ofert: {activatedOffersCount}");
            }
        }

        private bool TryActivateOffer(
            KeyValuePair<string, AllegroSaleOffer> offer,
            IDictionary<int, int> stany)
        {
            var canActivate = true; //base on offersOnline and its codes and Stany

            if(canActivate)
            {
                //TODO Activate via API
                offersOnline = offersOnline.Concat(new[] { offer }).ToList();
            }

            return canActivate;
        }
    }
}
