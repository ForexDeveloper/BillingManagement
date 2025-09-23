using Microsoft.EntityFrameworkCore;
using Shared.EventBus.Contracts;
using Shared.EventBus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public OutboxRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(OutboxEntity outboxEntity)
        {
            await _applicationDbContext.Outboxes.AddAsync(outboxEntity);
        }

        public async Task<List<OutboxEntity>> GetInProgressEventsAsync()
        {
            return await _applicationDbContext.Outboxes
                 .Where(p => p.Status == PublishStatus.InProgress)
                 .OrderBy(p => p.PublishTryCount)
                 .ThenBy(p => p.CreateDateTime).Take(200)
                 .ToListAsync();
        }

        public void Update(OutboxEntity outboxEntity)
        {
            _applicationDbContext.Outboxes.Update(outboxEntity);
        }

        public void UpdateRange(List<OutboxEntity> outboxEntities)
        {
            _applicationDbContext.Outboxes.UpdateRange(outboxEntities);
        }
    }

}
