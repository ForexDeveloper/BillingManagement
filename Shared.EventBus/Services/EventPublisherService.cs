using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.EventBus.Configurations;
using Shared.EventBus.Contracts;
using Shared.EventBus.Entitties;
using Shared.EventBus.Events;
using Shared.Logging.Abstraction.Extensions;
using Shared.Logging.Abstraction.Models;
using System.Diagnostics;
using System;

namespace Shared.EventBus.Services
{
    public class EventPublisherService : IEventPublisherService
    {
        private readonly IOptions<EventBusConfiguration> _config;
        public readonly IPublishEndpoint _publishEndpoint;
        public readonly ILogger<EventPublisherService> _logger;

        public EventPublisherService(IOptions<EventBusConfiguration> config, IPublishEndpoint publishEndpoint, ILogger<EventPublisherService> logger)
        {
            _config = config;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<bool> PublishAsync(OutboxEntity outboxEntity, CancellationToken cancellationToken)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                Type eventType = Type.GetType(outboxEntity.EventName);
                var deserializeEvent = System.Text.Json.JsonSerializer.Deserialize(outboxEntity.Message, eventType);
                await _publishEndpoint.Publish(deserializeEvent, cancellationToken);
                return true;
            }
            catch (Exception exception)
            {
                stopWatch.Stop();
                _logger.LogCritical(new LogStruct()
                {
                    Message = exception.Message,
                    ServiceName = nameof(PublishAsync),
                    InputParams = outboxEntity,
                    ResponseTimeStopWatcher = stopWatch,
                    Results = null,
                    Exception = exception,
                    Tags = LogMessageTag.EventBus
                });
                return false;
            }
        }

        public async Task<bool> PublishRangeAsync(List<OutboxEntity> outboxEntities, CancellationToken cancellationToken)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                var deserializeEvents = new List<object>();
                foreach (var item in outboxEntities)
                {
                    Type eventType = Type.GetType(item.EventName);

                    var deserializeEvent = System.Text.Json.JsonSerializer.Deserialize(item.Message, eventType);
                    deserializeEvents.Add(deserializeEvent);
                }
               
                await _publishEndpoint.PublishBatch(deserializeEvents, cancellationToken);
                return true;
            }
            catch (Exception exception)
            {
                stopWatch.Stop();
                _logger.LogCritical(new LogStruct()
                {
                    Message = exception.Message,
                    ServiceName = nameof(PublishRangeAsync),
                    InputParams = outboxEntities,
                    ResponseTimeStopWatcher = stopWatch,
                    Results = null,
                    Exception = exception,
                    Tags = LogMessageTag.EventBus
                });
                return false;
            }
        }
    }
}
