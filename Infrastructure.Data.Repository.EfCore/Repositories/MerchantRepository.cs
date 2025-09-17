using Domain.Core.Entities.MerchantAggregate;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class MerchantRepository : IMerchantRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public MerchantRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(Merchant merchant)
        {
            await _applicationDbContext.Merchants.AddAsync(merchant);
        }

        public async Task<bool> CheckMerchantByTenantIdAsync(int tenantId, List<int> merchantIds)
        {
            return await _applicationDbContext.Merchants.AnyAsync(p => merchantIds.Contains(p.Id) && p.TenantId == tenantId);
        }

        public async Task<Merchant> GetAsync(int id, int tenantId)
        {
            return await _applicationDbContext.Merchants
                //.Include(c => c.MerchantCategories)
                .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);
        }

        public async Task<Merchant> GetAsync(int id)
        {
            return await _applicationDbContext.Merchants.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Merchant> GetByIdAsync(int id)
        {
            return await _applicationDbContext.Merchants
                .Include(x => x.MerchantBranches)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public void Update(Merchant merchant)
        {
            _applicationDbContext.Merchants.Update(merchant);
        }

        public async Task<MerchantBranch> GetBranchAsync(int id)
        {
            return await _applicationDbContext.MerchantBranches.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<MerchantBranch> GetBranchByTerminalIdAsync(long terminalId)
        {
            return await _applicationDbContext.MerchantBranches
                .Include(b => b.Merchant)
                .FirstOrDefaultAsync(m => m.TerminalId == terminalId);
        }

        public async Task AddBranchAsync(MerchantBranch merchantBranch)
        {
            await _applicationDbContext.MerchantBranches.AddAsync(merchantBranch);
        }

        public void UpdateBranch(MerchantBranch merchantBranch)
        {
            _applicationDbContext.MerchantBranches.Update(merchantBranch);
        }

        public async Task<int> GetMerchantIdByBranchId(int? branchId)
        {
            return await _applicationDbContext.MerchantBranches.Where(p => p.Id == branchId).Select(p => p.MerchantId)
                .FirstOrDefaultAsync();
        }
    }
}
