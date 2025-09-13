using Domain.Core.Entities.FinancierAggregate;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class FinancierRepository : IFinancierRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public FinancierRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(Financier financier)
        {
            await _applicationDbContext.Financiers.AddAsync(financier);
        }

        public void Update(Financier financier)
        {
            _applicationDbContext.Financiers.Update(financier);
        }

        public async Task<Financier> GetAsync(int id)
        {
            return await _applicationDbContext.Financiers.Include(c=>c.CreditFlowConfigs).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> IsFinancierBelongToTenantAsync(int tenantId, int financierId)
        {
            return await _applicationDbContext.Financiers.AnyAsync(p => p.Id == financierId && p.TenantId == tenantId);
        }

        public async Task<Financier> GetByTenantIdAsync(int tenantId)
        {
            return await _applicationDbContext.Financiers.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.IsTenant);
        }
    }
}
