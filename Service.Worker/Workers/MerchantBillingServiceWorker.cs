using System;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Application.Service.Contracts;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Serilog.Extensions;
using Shared.Logging.Abstraction.Extensions;
using Microsoft.Extensions.DependencyInjection;

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
                    Results = "",
                    InputParams = "",
                    Exception = null,
                    Message = string.Empty,
                    ResponseTimeStopWatcher = _stopwatch,
                    ServiceName = $"{nameof(MerchantBillingServiceWorker)}_{nameof(ExecuteAsync)}",
                });

                _stopwatch.Reset();

                await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
            }
        }
    }
}