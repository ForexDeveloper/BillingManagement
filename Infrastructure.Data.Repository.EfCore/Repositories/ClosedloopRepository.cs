using Domain.Core.Entities.ClosedloopAggregate;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class ClosedloopRepository : IClosedloopRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public ClosedloopRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(Closedloop cmd)
        {
            await _applicationDbContext.Closedloops.AddAsync(cmd);
        }

        public async Task<Closedloop> GetByIdAsync(int id)
        => await _applicationDbContext.Closedloops
                .Include(c => c.ClosedloopCategories)
                .Include(c => c.ClosedloopMerchants)
                .Include(c => c.WalletConfiguration)
                .FirstOrDefaultAsync(p => p.Id == id);

        public async Task<bool> ChaeckClosedloopAsync(int id)
        => await _applicationDbContext.Closedloops.AnyAsync(p => p.Id == id);

        public void Update(Closedloop cmd)
        {
            _applicationDbContext.Closedloops.Update(cmd);
        }

        public async Task<bool> CheckClosedloop(int walletConfigurationId, List<int> closedloops)
        {
            return await _applicationDbContext.Closedloops.AnyAsync(c =>
            c.WalletConfigurationId == walletConfigurationId &&
            closedloops.Contains(c.Id)
            );
        }
    }

}
