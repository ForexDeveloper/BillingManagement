using Application.Service.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.EventBus.Configurations;
using Shared.Logging.Abstraction.Extensions;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Serilog.Extensions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Worker.Workers;

public class UpdateBillingServiceWorker : BackgroundService
{
    private readonly EventBusConfiguration _config;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _services;

    public UpdateBillingServiceWorker(IOptions<EventBusConfiguration> config, IServiceProvider services, IConfiguration configuration)
    {
        _services = services;
        _config = config.Value;
        _configuration = configuration;
    }

    private readonly Stopwatch _stopwatch = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _stopwatch.Start();

            using IServiceScope scope = _services.CreateScope();
            var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<UpdateBillingServiceWorker>>();

            logger.AddTraceId(Guid.NewGuid().ToString());

            try
            {
                var currentTime = DateTime.Now.TimeOfDay;
                var startTime = new TimeSpan(0, 0, 0);    // 00:00 AM
                var endTime = new TimeSpan(0, 15, 0);     // 00:15 AM

                bool isInRange = currentTime >= startTime && currentTime <= endTime;

                if (!isInRange)
                    await billingService.UpdateBillings();

            }
            catch (Exception exception)
            {
                logger.LogCritical(new LogStruct()
                {
                    ServiceName = $"UpdateBillingServiceWorker_{nameof(ExecuteAsync)}",
                    InputParams = "",
                    ResponseTimeStopWatcher = _stopwatch,
                    Results = "",
                    Exception = exception,
                    Message = exception.Message
                });
            }
            finally
            {
                //logger.LogTrace(new LogStruct()
                //{
                //    ServiceName = $"UpdateBillingServiceWorker_{nameof(ExecuteAsync)}",
                //    InputParams = "",
                //    ResponseTimeStopWatcher = _stopwatch,
                //    Results = "",
                //    Exception = null,
                //    Message = string.Empty
                //});
                _stopwatch.Reset();
                await Task.Delay(int.Parse(_configuration["BillingJobConfiguration:MillisecondsDelay"]), stoppingToken); //todo from config
            }
        }
    }
}
