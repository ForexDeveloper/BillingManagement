using Application.Service.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Logging.Abstraction.Extensions;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Serilog.Extensions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Worker.Workers;

public class MerchantBillingServiceWorker(IServiceProvider services) : BackgroundService
{
    private readonly Stopwatch _stopwatch = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = services.CreateScope();

        while (!stoppingToken.IsCancellationRequested)
        {
            _stopwatch.Start();

            var backgroundJobService = scope.ServiceProvider.GetRequiredService<IBackgroundJobService>();
            var merchantBillingService = scope.ServiceProvider.GetRequiredService<IMerchantBillingService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<MerchantBillingServiceWorker>>();

            logger.AddTraceId(Guid.NewGuid().ToString());

            try
            {
                var jobCreatedDateTime = await backgroundJobService.CreateMerchantBillingJobAsync(stoppingToken);

                await merchantBillingService.IssueOrOverdueBilling(jobCreatedDateTime, stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogCritical(new LogStruct()
                {
                    Results = "",
                    InputParams = "",
                    Exception = exception,
                    Message = exception.Message,
                    ResponseTimeStopWatcher = _stopwatch,
                    ServiceName = $"{nameof(MerchantBillingServiceWorker)}_{nameof(ExecuteAsync)}",
                });
            }
            finally
            {
                logger.LogTrace(new LogStruct()
                {
                    ResponseTimeStopWatcher = _stopwatch,
                    Message = "MerchantBillingServiceWorker executed successfully",
                    ServiceName = $"{nameof(MerchantBillingServiceWorker)}_{nameof(ExecuteAsync)}",
                });

                _stopwatch.Reset();

                await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
            }
        }
    }
}