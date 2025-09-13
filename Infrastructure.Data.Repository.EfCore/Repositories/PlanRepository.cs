using Domain.Core.Entities.PlanAggregate;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class PlanRepository : IPlanRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public PlanRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(Plan cmd)
        {
            await _applicationDbContext.Plans.AddAsync(cmd);
        }

        public async Task<bool> CheckPlanAsync(int id)
        {
            return await _applicationDbContext.Plans.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> CheckPlansExistAsync(List<int> ids)
        {
            var count = await _applicationDbContext.Plans.CountAsync(p => ids.Contains(p.Id));

            return count == ids.Count;
        }

        public async Task<bool> CheckPlansBelongToTenantAsync(List<int> ids, int tenantId)
        {
            var count = await _applicationDbContext.Plans.Where(p => p.WalletConfiguration.TenantId == tenantId).CountAsync(p => ids.Contains(p.Id));

            return count == ids.Count;
        }

        public async Task<Plan> GetByIdAsync(int id)
        {
            return await _applicationDbContext.Plans
                .Include(c => c.PlanDetails.Where(d => !d.IsDeleted))
                .ThenInclude(c => c.PlanDetailInstallments.Where(d => !d.IsDeleted))
                .Include(c => c.WalletConfiguration)
                .Include(c => c.PlanClosedloops.Where(d => !d.IsDeleted))
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<PlanDetailInstallment>> GetPlanDetailInstallmentsAsync(List<int> planDetailInstallmentIds)
        {
            return await _applicationDbContext.PlanDetailInstallments
                .Include(c => c.PlanDetail).ThenInclude(x => x.Plan)
                .Where(c => planDetailInstallmentIds.Contains(c.Id))
                .ToListAsync();
        }

        public void Update(Plan cmd)
        {
            _applicationDbContext.Plans.Update(cmd);
        }

        public async Task<bool> HasWalletContractPlanByPlanId(int id)
        => await _applicationDbContext.Plans
                .AnyAsync(c => c.Id == id && c.WalletContractPlans.Any(c => c.PlanId == id));

        public async Task<bool> CheckPlanMaxWalletAsync(int walletConfigurationId, decimal maxWallet)
        => await _applicationDbContext.Plans.Where(p => p.WalletConfigurationId == walletConfigurationId &&
        p.MaxWallet > maxWallet).AnyAsync();

        public async Task<bool> CheckPlanMaxTotalCreditAsync(int walletConfigurationId, decimal maxTotalCredit)
       => await _applicationDbContext.Plans.Where(p => p.WalletConfigurationId == walletConfigurationId &&
       p.MaxTotalCredit > maxTotalCredit).AnyAsync();
    }

}
