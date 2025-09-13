using Domain.Core.UnitOfWorkContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.EventBus.Configurations;
using Shared.EventBus.Contracts;
using Shared.Logging.Abstraction.Extensions;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Serilog.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Worker.Workers
{
    public class OutboxPublisherServiceWorker : BackgroundService
    {
        private readonly EventBusConfiguration _config;
        private readonly IServiceProvider _services;

        public OutboxPublisherServiceWorker(IOptions<EventBusConfiguration> config, IServiceProvider services)
        {
            _services = services;
            _config = config.Value;
        }

        private readonly Stopwatch _stopwatch = new Stopwatch();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var fetchInterval = _config.DefaultEventFetchInterval;
            while (!stoppingToken.IsCancellationRequested)
            {

                _stopwatch.Start();

                using IServiceScope scope = _services.CreateScope();
                var outBoxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IApplicationDbContextUnitOfWork>();

                var logger = scope.ServiceProvider.GetRequiredService<ILogger<OutboxPublisherServiceWorker>>();
                var eventPublisherService = scope.ServiceProvider.GetRequiredService<IEventPublisherService>();

                logger.AddTraceId(Guid.NewGuid().ToString());

                try
                {
                    var inProgressOutboxEvents = await outBoxRepository.GetInProgressEventsAsync();
                    if (inProgressOutboxEvents.Any())
                    {
                        var succeed = await eventPublisherService.PublishRangeAsync(inProgressOutboxEvents, stoppingToken);

                        foreach (var eventItem in inProgressOutboxEvents)
                        {
                            eventItem.PrepareToPublish();
                            eventItem.GotoProperState(succeed);
                        }
                        outBoxRepository.UpdateRange(inProgressOutboxEvents);
                        await unitOfWork.SaveChangesAsync(stoppingToken);
                        fetchInterval = _config.DefaultEventFetchInterval;
                    }
                    else
                    {
                        if (fetchInterval <= 5)
                        {
                            fetchInterval = fetchInterval + 2;
                        }
                    }
                }
                catch (Exception exception)
                {
                    logger.LogCritical(new LogStruct()
                    {
                        ServiceName = $"OutboxPublisherServiceWorker_{nameof(ExecuteAsync)}",
                        InputParams = "",
                        ResponseTimeStopWatcher = _stopwatch,
                        Results = "",
                        Exception = exception,
                        Message = exception.Message
                    });
                }
                finally
                {
                    _stopwatch.Reset();
                    await Task.Delay(fetchInterval * 1000, stoppingToken);
                }

            }
        }

    }
}