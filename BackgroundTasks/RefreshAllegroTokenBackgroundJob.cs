using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using RestSharp;
using react_app.Allegro;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace react_app.BackgroundTasks
{
    [DisallowConcurrentExecution]
    public class RefreshAllegroTokenBackgroundJob : IJob
    {
        private readonly ILogger<RefreshAllegroTokenBackgroundJob> _logger;
        private readonly IOptions<AllegroSettings> allegroSettings;
        private readonly IWebHostEnvironment env;

        public RefreshAllegroTokenBackgroundJob(
            ILogger<RefreshAllegroTokenBackgroundJob> logger,
            IOptions<AllegroSettings> allegroSettings,
            IWebHostEnvironment env)
        {
            _logger = logger;
            this.allegroSettings = allegroSettings;
            this.env = env;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if (!File.Exists(Path.Combine(env.ContentRootPath, "refresh-token")))
            {
                _logger.LogError($"Brak tokenu. Zaloguj się do Allegro");
                return;
            }

            var refreshToken = File.ReadAllText(Path.Combine(env.ContentRootPath, "refresh-token"));

            var client = new RestClient("https://allegro.pl");

            var request = new RestRequest($"/auth/oauth/token", Method.POST);
            request.AddHeader("Authorization", $"Basic {allegroSettings.Value.Base64Bearer}");
            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("refresh_token", refreshToken);

            var response = await client.ExecuteAsync<AllegroAccessTokenR>(request);

            if (!response.Data.IsValid)
            {
                _logger.LogError(response.Content);
                return;
            }

            File.WriteAllText(Path.Combine(env.ContentRootPath, "token"), response.Data.AccessToken);
            File.WriteAllText(Path.Combine(env.ContentRootPath, "refresh-token"), response.Data.RefreshToken);

            _logger.LogInformation($"Token Allegro zaktualizowany");
        }
    }
}
