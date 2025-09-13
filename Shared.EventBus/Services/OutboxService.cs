using Microsoft.Extensions.Options;
using Shared.EventBus.Configurations;
using Shared.EventBus.Contracts;
using Shared.EventBus.Entitties;
using System.Text.Json;

namespace Shared.EventBus.Services
{
    public class OutboxService : IOutboxService
    {
        private readonly IOutboxRepository _outboxRepository;

        private readonly IOptions<EventBusConfiguration> _config;

        public OutboxService(IOutboxRepository outboxRepository, IOptions<EventBusConfiguration> config)
        {
            _outboxRepository = outboxRepository;
            _config = config;
        }

        public async void AddNewEvent<T>(T message) where T : IEventSign
        {
            var serilizedMessage = System.Text.Json.JsonSerializer.Serialize(message);
            var outBox = new OutboxEntity($"{message.GetType().FullName}", serilizedMessage);
            await _outboxRepository.AddAsync(outBox);
        }
    }
}
