using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Microsoft.Extensions.DependencyInjection;
using System;
using react_app.Services;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace react_app.BackgroundTasks
{
    [DisallowConcurrentExecution]
    public class OrdersSyncBackgroundJob : IJob
    {
        private readonly ILogger<OrdersSyncBackgroundJob> _logger;
        private readonly IServiceProvider serviceProvider;
        private readonly IWebHostEnvironment env;
        private readonly IOptions<Settings> settings;

        public OrdersSyncBackgroundJob(
            ILogger<OrdersSyncBackgroundJob> logger,
            IServiceProvider serviceProvider,
            IWebHostEnvironment env,
            IOptions<Settings> settings)
        {
            _logger = logger;
            this.serviceProvider = serviceProvider;
            this.env = env;
            this.settings = settings;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            if(DateTime.Now < settings.Value.StartOrdersSyncFrom)
            {
                _logger.LogInformation($"Startuję dopiero {settings.Value.StartOrdersSyncFrom}");
                return;
            }

            using (var scope = serviceProvider.CreateScope())
            {
                _logger.LogInformation($"Synchronizacja wydań rozpoczęta");

                var orderService = scope.ServiceProvider.GetService<OrderService>();
                var syncCount = await orderService.SyncOrdersAsync();

                _logger.LogInformation($"Synchronizacja wydań zakończona powodzeniem. Liczba zsynchronizowanych towarów: {syncCount}");
            }
        }
    }
}
