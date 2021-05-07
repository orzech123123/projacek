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
using react_app.Utils;
using Microsoft.Extensions.Options;
using react_app.Configuration;

namespace react_app.BackgroundTasks
{
    [DisallowConcurrentExecution]
    public class BackupAndSendDatabasesJob : IJob
    {
        private const int MaxOfferAmount = 10;

        private readonly ILogger<BackupAndSendDatabasesJob> _logger;
        private readonly IServiceProvider serviceProvider;
        private readonly IOptions<Settings> settings;
        private readonly EmailService emailService;
        private readonly CommandExecutor commandExecutor;

        private const string backupsPath = "/home/sql-server-volume/backups/";
        private static string tempPath = $"{backupsPath}temp/";

        public BackupAndSendDatabasesJob(
            ILogger<BackupAndSendDatabasesJob> logger,
            EmailService emailService,
            CommandExecutor commandExecutor,
            IServiceProvider serviceProvider,
            IOptions<Settings> settings)
        {
            _logger = logger;
            this.emailService = emailService;
            this.commandExecutor = commandExecutor;
            this.serviceProvider = serviceProvider;
            this.settings = settings;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                _logger.LogInformation($"Rozpoczęto archiwizację baz danych");

                var dbContext = scope.ServiceProvider.GetService<LomagDbContext>();

                CreateTempDirectory();

                await BackupDatabasesAsync(dbContext, tempPath);
                await BackupDatabasesAsync(dbContext, backupsPath);

                var zipFilename = ZipBackups();

                await SendEmailsAsync(zipFilename);

                Clean(zipFilename);

                _logger.LogInformation($"Zarchiwizowano bazy danych i wysłano za pośrednictwem e-maila");
            }
        }

        private void Clean(string zipFilename)
        {
            Directory.Delete(tempPath, true);
            File.Delete(zipFilename);
        }

        private async Task SendEmailsAsync(string zipFilename)
        {
            var emails = settings.Value.EmailsToSend.Split(' ');
            foreach (var email in emails)
            {
                await emailService.SendEmailAsync(email, "PROJACK - archiwa baz danych", "Jak zwykle o tej porze, przesyłam zarchiwizowane bazy danych.", zipFilename);
            }
        }

        private string ZipBackups()
        {
            var zipFilename = $"{backupsPath}backups-{DateTime.Now:yyyy-MM-dd_HH:mm:ss}.zip";
            ZipFile.CreateFromDirectory(tempPath, zipFilename);

            return zipFilename;
        }

        private void CreateTempDirectory()
        {
            Directory.CreateDirectory(tempPath);
            commandExecutor.Permission("-R 777", tempPath);
        }

        private async Task BackupDatabasesAsync(DbContext dbContext, string path)
        {
            var cmd = dbContext.Database.GetDbConnection().CreateCommand();

            cmd.CommandText = "dbo.sp_BackupDatabases";
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@backupLocation", SqlDbType.VarChar) { Value = path });
            cmd.Parameters.Add(new SqlParameter("@backupType", SqlDbType.VarChar) { Value = "F" });

            if (cmd.Connection.State != ConnectionState.Open)
                cmd.Connection.Open();

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
