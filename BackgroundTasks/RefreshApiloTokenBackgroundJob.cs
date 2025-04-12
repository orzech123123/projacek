using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using RestSharp;
using react_app.Allegro;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using react_app.Apilo;

namespace react_app.BackgroundTasks
{
    [DisallowConcurrentExecution]
    public class RefreshApiloTokenBackgroundJob : IJob
    {
        private readonly ILogger<RefreshAllegroTokenBackgroundJob> _logger;
        private readonly IOptions<ApiloSettings> _apiloSettings; 
        private readonly IWebHostEnvironment env;

        public RefreshApiloTokenBackgroundJob(
            ILogger<RefreshAllegroTokenBackgroundJob> logger,
            IOptions<ApiloSettings> apiloSettings, 
            IWebHostEnvironment env)
        {
            _logger = logger;
            _apiloSettings = apiloSettings; 
            this.env = env;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var hasRefreshToken = File.Exists(Path.Combine(env.ContentRootPath, "refresh-token"));

            if (!hasRefreshToken)
            {
                _logger.LogError($"Brak tokenu Apilo. Pozyskiwanie pierwszego tokena i refresh_tokena Apilo");
            }

            var token = hasRefreshToken ? File.ReadAllText(Path.Combine(env.ContentRootPath, "refresh-token")) : _apiloSettings.Value.AuthCode;
            var grantType = hasRefreshToken ? "refresh_token" : "authorization_code";

            var client = new RestClient(_apiloSettings.Value.Url);

            var request = new RestRequest($"/rest/auth/token/", Method.POST);

            request.AddHeader("Authorization", $"Basic {_apiloSettings.Value.Base64Bearer}");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Accept", "application/json");

            var body = new
            {
                grantType,
                token
            };

            request.AddJsonBody(body);

            var response = await client.ExecuteAsync<ApiloAccessTokenResponse>(request);

            if (response.IsSuccessful)
            {
                if (response.Data.IsValid)
                {
                    var accessToken = response.Data.AccessToken;
                    var refreshToken = response.Data.RefreshToken;

                    File.WriteAllText(Path.Combine(env.ContentRootPath, "token"), accessToken);
                    File.WriteAllText(Path.Combine(env.ContentRootPath, "refresh-token"), refreshToken);

                    _logger.LogInformation($"Token Apilo zaktualizowany");
                }
                else
                {
                    _logger.LogError($"Nieprawidłowy format odpowiedzi z Apilo: {response.Content}");
                }
            }
            else
            {
                _logger.LogError($"Błąd podczas pozyskiwania pierwszego tokenu Apilo: {response.ErrorMessage}, Status Code: {response.StatusCode}, Content: {response.Content}");
            }
        }
    }
}