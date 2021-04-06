using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Microsoft.Extensions.DependencyInjection;
using System;
using react_app.Services;
using react_app.Allegro;
using System.Linq;

namespace react_app.BackgroundTasks
{
    [DisallowConcurrentExecution]
    public class ActivateInactiveAllegroOffersJob : IJob
    {
        private readonly ILogger<ActivateInactiveAllegroOffersJob> _logger;
        private readonly IServiceProvider serviceProvider;
        private readonly AllegroOfferService allegroOfferService;

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

                foreach (var offer in offers)
                {
                    var codes = lomagService.ExtractCodes(offer.Value.External?.Id, lomagKodyKreskowe, 1);
                    if(codes.Any())
                    {

                    }
                }

                _logger.LogInformation($"Aktywacja nieaktywnych ofert zakończona powidzeniem. Aktywowano ofert: {0}");
            }
        }
    }
}
