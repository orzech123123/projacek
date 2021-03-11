using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Microsoft.Extensions.DependencyInjection;
using System;
using react_app.Services;
using Microsoft.Extensions.Options;
using react_app.Configuration;

namespace react_app.BackgroundTasks
{
    [DisallowConcurrentExecution]
    public class OrdersSyncBackgroundJob : IJob
    {
        private readonly ILogger<OrdersSyncBackgroundJob> _logger;
        private readonly IServiceProvider serviceProvider;
        private readonly IOptions<Settings> settings;

        public OrdersSyncBackgroundJob(
            ILogger<OrdersSyncBackgroundJob> logger,
            IServiceProvider serviceProvider,
            IOptions<Settings> settings)
        {
            _logger = logger;
            this.serviceProvider = serviceProvider;
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
