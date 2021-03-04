using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Microsoft.Extensions.DependencyInjection;
using System;
using react_app.Services;

namespace react_app.BackgroundTasks
{
    [DisallowConcurrentExecution]
    public class OrdersSyncBackgroundJob : IJob
    {
        private readonly ILogger<OrdersSyncBackgroundJob> _logger;
        private readonly IServiceProvider serviceProvider;

        public OrdersSyncBackgroundJob(ILogger<OrdersSyncBackgroundJob> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            this.serviceProvider = serviceProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var orderService = scope.ServiceProvider.GetService<OrderService>();
                await orderService.SyncOrdersAsync();

                _logger.LogInformation($"OK - {DateTime.Now}");
            }
        }
    }
}
