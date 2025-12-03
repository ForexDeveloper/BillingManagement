using System;
using System.Threading;
using System.Diagnostics;
using Service.Worker.Config;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Application.Service.Contracts;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Serilog.Extensions;
using Shared.Logging.Abstraction.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Service.Worker.Workers;

public sealed class MerchantBillingServiceWorker(IServiceProvider services,
    IOptions<MerchantBillingJobConfiguration> options) : BackgroundService
{
    private readonly Stopwatch _stopwatch = new();
    private readonly MerchantBillingJobConfiguration _configuration = options.Value;

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

                await Task.Delay(TimeSpan.FromMinutes(_configuration.DelayInMinute), stoppingToken);
            }
        }
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MerchantBillingServiceWorker>>();

        logger.LogTrace(new LogStruct()
        {
            ResponseTimeStopWatcher = _stopwatch,
            Message = "MerchantBillingServiceWorker StartAsync",
            ServiceName = $"{nameof(MerchantBillingServiceWorker)}_{nameof(StartAsync)}"
        });

        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        using var scope = services.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<MerchantBillingServiceWorker>>();

        logger.LogCritical(new LogStruct()
        {
            ResponseTimeStopWatcher = _stopwatch,
            Message = "MerchantBillingServiceWorker StopAsync",
            ServiceName = $"{nameof(MerchantBillingServiceWorker)}_{nameof(StopAsync)}"
        });

        return base.StopAsync(cancellationToken);
    }
}