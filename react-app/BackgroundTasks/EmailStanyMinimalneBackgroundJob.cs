using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Microsoft.Extensions.DependencyInjection;
using System;
using react_app.Services;
using Microsoft.Extensions.Options;
using System.Linq;

namespace react_app.BackgroundTasks
{
    [DisallowConcurrentExecution]
    public class EmailStanyMinimalneBackgroundJob : IJob
    {
        private readonly ILogger<EmailStanyMinimalneBackgroundJob> _logger;
        private readonly IServiceProvider serviceProvider;

        public EmailStanyMinimalneBackgroundJob(
            ILogger<EmailStanyMinimalneBackgroundJob> logger,
            IServiceProvider serviceProvider,
            IOptions<Settings> settings)
        {
            _logger = logger;
            this.serviceProvider = serviceProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var lomagService = scope.ServiceProvider.GetService<LomagService>();

                var stany = lomagService.GetWolnePrzyjecia();
                var towary = await lomagService.GetTowary();

                var przekroczoneStanyMin = towary
                    .Select(t => new
                    {
                        t.StanMinimalny,
                        t.KodKreskowy,
                        Stan = stany[t.IdTowaru] != null ?
                            stany[t.IdTowaru].Sum(st => st.Ilosc - (st.Wydano ?? 0)) :
                            0
                    })
                    .Where(t => t.Stan < t.StanMinimalny);

                _logger.LogInformation($"Emaile o stanach minimalnych zostały wysłane");
            }
        }
    }
}
