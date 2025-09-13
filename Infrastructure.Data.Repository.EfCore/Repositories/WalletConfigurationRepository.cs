using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class WalletConfigurationRepository : IWalletConfigurationRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public WalletConfigurationRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(WalletConfiguration walletConfiguration)
        {
            await _applicationDbContext.WalletConfigurations.AddAsync(walletConfiguration);
        }

        public async Task<bool> CheckWalletConfigurationByTenantIdAsync(int id, int tenantId)
        => await _applicationDbContext.WalletConfigurations.AnyAsync(c => c.Id == id && c.TenantId == tenantId);

        public async Task<WalletConfiguration> GetByIdAsync(int id)
        {
            return await _applicationDbContext.WalletConfigurations.FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<WalletConfiguration> GetByTenantIdAsync(int id, int? tenantId = null)
                => await _applicationDbContext.WalletConfigurations.FirstOrDefaultAsync(c => c.Id == id &&
                (!tenantId.HasValue || c.TenantId == tenantId));
        public void Update(WalletConfiguration WalletConfiguration)
        {
            _applicationDbContext.WalletConfigurations.Update(WalletConfiguration);
        }

        public async Task<bool> HasPlanAsync(int id)
         => await _applicationDbContext.WalletConfigurations
            .AnyAsync(c => c.Id == id && c.Plans.Any(d => d.WalletConfigurationId == id));
       
    }

}
