using Shared.EventBus.Entitties;

namespace Shared.EventBus.Contracts
{
    public interface IEventPublisherService
    {
        public Task<bool> PublishAsync(OutboxEntity outboxEntity, CancellationToken cancellationToken);
        public Task<bool> PublishRangeAsync(List<OutboxEntity> outboxEntities, CancellationToken cancellationToken);

    }
}
