using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Microsoft.Extensions.DependencyInjection;
using System;
using react_app.Services;
using Microsoft.Extensions.Options;
using System.Linq;
using react_app.Configuration;

namespace react_app.BackgroundTasks
{
    [DisallowConcurrentExecution]
    public class EmailMinimumInventoryBackgroundJob : IJob
    {
        private readonly ILogger<EmailMinimumInventoryBackgroundJob> _logger;
        private readonly IServiceProvider serviceProvider;
        private readonly IOptions<Settings> settings;
        private readonly EmailService emailService;

        public EmailMinimumInventoryBackgroundJob(
            ILogger<EmailMinimumInventoryBackgroundJob> logger,
            IServiceProvider serviceProvider,
            IOptions<Settings> settings,
            EmailService emailService)
        {
            _logger = logger;
            this.serviceProvider = serviceProvider;
            this.settings = settings;
            this.emailService = emailService;
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
                        t.Nazwa,
                        t.KodKreskowy,
                        Stan = stany[t.IdTowaru] != null ?
                            stany[t.IdTowaru].Sum(st => st.Ilosc - (st.Wydano ?? 0)) :
                            0
                    })
                    .Where(t => t.Stan < t.StanMinimalny)
                    .OrderBy(t => t.StanMinimalny);

                var przekroczoneStanyMinStr = string.Join("<br /><br />", przekroczoneStanyMin
                    .Select(s => $"{s.Nazwa} - {s.KodKreskowy} - stan: {s.Stan} - stan min. - {s.StanMinimalny}"));

                var emails = settings.Value.EmailsToSend.Split(' ');
                foreach (var email in emails)
                {
                    await emailService.SendEmailAsync(email, "PROJACK - Informacja o niskich stanach magazynowych", przekroczoneStanyMinStr);
                }

                _logger.LogInformation($"Emaile o stanach minimalnych zostały wysłane");
            }
        }
    }
}
