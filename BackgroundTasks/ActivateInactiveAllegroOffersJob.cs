using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Microsoft.Extensions.DependencyInjection;
using System;
using react_app.Services;
using react_app.Allegro;
using System.Linq;
using System.Collections.Generic;
using react_app.Lomag;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace react_app.BackgroundTasks
{
    [DisallowConcurrentExecution]
    public class ActivateInactiveAllegroOffersJob : IJob
    {
        private const int MaxOfferAmount = 10;

        private readonly ILogger<ActivateInactiveAllegroOffersJob> _logger;
        private readonly IServiceProvider serviceProvider;
        private readonly AllegroOfferService allegroOfferService;
        private readonly EmailService emailService;
        private IEnumerable<(AllegroSaleOffer Offer, IEnumerable<string> Codes)> offersOnline;

        public ActivateInactiveAllegroOffersJob(
            ILogger<ActivateInactiveAllegroOffersJob> logger,
            IServiceProvider serviceProvider,
            AllegroOfferService allegroOfferService,
            EmailService emailService)
        {
            _logger = logger;
            this.serviceProvider = serviceProvider;
            this.allegroOfferService = allegroOfferService;
            this.emailService = emailService;
        }

        private static void Exec(string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\""
                }
            };

            process.Start();
            process.WaitForExit();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                //TODO
                var c = scope.ServiceProvider.GetService<LomagDbContext>();
                DbCommand cmd = c.Database.GetDbConnection().CreateCommand();

                cmd.CommandText = "dbo.sp_BackupDatabases";
                cmd.CommandType = CommandType.StoredProcedure;


                Directory.CreateDirectory("/home/sql-server-volume/backups/temp/");
                Exec("chmod -R 777 /home/sql-server-volume/backups/temp/");

                cmd.Parameters.Add(new SqlParameter("@backupLocation", SqlDbType.VarChar) { Value = "/home/sql-server-volume/backups/temp/" });
                cmd.Parameters.Add(new SqlParameter("@backupType", SqlDbType.VarChar) { Value = "F" });

                if (cmd.Connection.State != ConnectionState.Open)
                    cmd.Connection.Open(); 

                await cmd.ExecuteNonQueryAsync();

                var zipFilename = $"/home/sql-server-volume/backups/archive{Guid.NewGuid()}.zip";
                ZipFile.CreateFromDirectory("/home/sql-server-volume/backups/temp/", zipFilename);
                Directory.Delete("/home/sql-server-volume/backups/temp/", true);

                await emailService.SendEmailAsync("michalorzechowski123@gmail.com", "BAZKI", "bazki", zipFilename);
                //TODO

                _logger.LogInformation($"Aktywacja nieaktywnych ofert zakończona powodzeniem. " +
                    $"Aktywowano ofert: {0}");

                return;


                var lomagService = scope.ServiceProvider.GetService<LomagService>();

                var towary = await lomagService.GetTowary();
                var stany = lomagService.GetStany(t => t.Towar.KodKreskowy);
                var offers = allegroOfferService.GetAll();

                var lomagKodyKreskowe = towary.Select(t => t.KodKreskowy);

                var offersWithCodes = offers
                    .Select(o => 
                    (
                        Offer: o.Value,
                        Codes: lomagService.ExtractCodes(o.Value.Signature, lomagKodyKreskowe, 1)
                    ))
                    .Where(o => o.Codes.Any())
                    .ToList();

                offersOnline = offersWithCodes
                    .Where(o => o.Offer.Publication.Status != AllegroSaleOfferStatus.Ended)
                    .ToList();

                var offersToTryActivate = offersWithCodes
                    .Where(o => o.Offer.Publication.Status == AllegroSaleOfferStatus.Ended)
                    //.Where(o => o.Offer.Id == "10601294507") //TODO REMOVE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    .ToList();

                var activatedOfferUrls = offersToTryActivate
                    .Select(o => TryActivateOffer(o, stany))
                    .Where(o => o != null)
                    .Select(o => $"https://allegro.pl/oferta/{o.Id}")
                    .ToList();

                var activatedOfferUrlsJoined = activatedOfferUrls.Any() ? $"[{string.Join(", ", activatedOfferUrls)}]" : null;

                _logger.LogInformation($"Aktywacja nieaktywnych ofert zakończona powodzeniem. " +
                    $"Aktywowano ofert: {activatedOfferUrls.Count()}\n{activatedOfferUrlsJoined}");
            }
        }

        private AllegroSaleOffer TryActivateOffer(
            (AllegroSaleOffer Offer, IEnumerable<string> Codes) offerWithCodes,
            IDictionary<string, int> stany)
        {
            var offer = offerWithCodes.Offer;
            var lastValidAmount = 0;

            while(true)
            {
                var stanExceeded = false;

                foreach (var code in offerWithCodes.Codes)
                {
                    var codeStan = stany.ContainsKey(code) ? stany[code] : 0;
                    var codeOnlineAmount = offersOnline.Sum(o => o.Offer.Stock.Available * o.Codes.Count(c => c == code));
                    codeStan -= codeOnlineAmount;

                    if(lastValidAmount + 1 > codeStan)
                    {
                        stanExceeded = true;
                        break;
                    }
                }

                if(stanExceeded)
                {
                    break;
                }

                lastValidAmount++;
            }

            if (lastValidAmount > 0)
            {
                var finalNumberToActivate = Math.Min(lastValidAmount, MaxOfferAmount);

                offer = allegroOfferService.Update(offer, finalNumberToActivate);
                offer = allegroOfferService.Activate(offer);

                offerWithCodes.Offer = offer;
                offersOnline = offersOnline.Concat(new[] { offerWithCodes });

                return offer;
            }

            return null;
        }
    }
}
