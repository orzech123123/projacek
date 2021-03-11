using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Microsoft.Extensions.DependencyInjection;
using System;
using react_app.Wmprojack;
using Microsoft.EntityFrameworkCore;

namespace react_app.BackgroundTasks
{
    [DisallowConcurrentExecution]
    public class TruncateLogsBackgroundJob : IJob
    {
        private readonly ILogger<TruncateLogsBackgroundJob> _logger;
        private readonly IServiceProvider serviceProvider;

        public TruncateLogsBackgroundJob(
            ILogger<TruncateLogsBackgroundJob> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            this.serviceProvider = serviceProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var wmprojackDbContext = scope.ServiceProvider.GetService<WmprojackDbContext>();

                await wmprojackDbContext.Database.ExecuteSqlRawAsync(
                   $"delete FROM[WmProJack].[dbo].[LogEvents] where TimeStamp < " +
                   $"'{DateTime.Now.AddHours(-48):yyyy-MM-dd HH:mm:ss}'");

                _logger.LogInformation($"Czyszczenie logów zakończone powodzeniem");
            }
        }
    }
}
