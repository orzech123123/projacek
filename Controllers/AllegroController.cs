using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using react_app.Allegro;
using RestSharp;

namespace react_app.Controllers
{
    [ApiController]
    public class AllegroController : ControllerBase
    {
        private readonly IOptions<AllegroSettings> allegroSettings;
        private readonly IWebHostEnvironment env;

        public AllegroController(IOptions<AllegroSettings> allegroSettings, IWebHostEnvironment env)
        {
            this.allegroSettings = allegroSettings;
            this.env = env;
        }


        [HttpGet("/[controller]/settings")]
        public IActionResult GetSettings()
        {
            return Ok(new
            {
                clientId = allegroSettings.Value.ClientId,
                returnUrl = allegroSettings.Value.ReturnUrl
            });
        }


        [HttpGet("/[controller]")]
        public async Task<IActionResult> UpdateAccessToken(string code)
        {
            var client = new RestClient("https://allegro.pl");

            var request = new RestRequest($"/auth/oauth/token", Method.POST);
            request.AddHeader("Authorization", $"Basic {allegroSettings.Value.Base64Bearer}");
            request.AddParameter("grant_type", "authorization_code");
            request.AddParameter("code", code);
            request.AddParameter("redirect_uri", allegroSettings.Value.ReturnUrl);
            var response = await client.ExecuteAsync<AllegroAccessTokenR>(request);

            System.IO.File.WriteAllText(Path.Combine(env.ContentRootPath, "token"), response.Data.AccessToken);
            System.IO.File.WriteAllText(Path.Combine(env.ContentRootPath, "refresh-token"), response.Data.RefreshToken);

            return new RedirectResult("/");
        }

       
    }
}
