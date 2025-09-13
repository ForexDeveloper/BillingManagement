using Shared.EventBus.Entitties;

namespace Shared.EventBus.Contracts
{
    public interface IOutboxRepository
    {
        Task AddAsync(OutboxEntity outboxEntity);
        void Update(OutboxEntity outboxEntity);
        void UpdateRange(List<OutboxEntity> outboxEntities);
        Task<List<OutboxEntity>> GetInProgressEventsAsync();
    }
}
