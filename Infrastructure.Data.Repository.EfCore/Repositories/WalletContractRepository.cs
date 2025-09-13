using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class WalletContractRepository : IWalletContractRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public WalletContractRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task AddAsync(WalletContract contract)
        {
            await _applicationDbContext.WalletContracts.AddAsync(contract);
        }

        public async Task AddWalletContractBusinessIdentityAsync(WalletContractBusinessIdentity walletContractBusinessIdentity)
        {
            await _applicationDbContext.WalletContractBusinessIdentities.AddAsync(walletContractBusinessIdentity);
        }

        public void Update(WalletContract contract)
        {
            _applicationDbContext.WalletContracts.Update(contract);
        }

        public async Task<WalletContract> GetAsync(int id, int? tenantId = null)
        {
            var contract = await _applicationDbContext.WalletContracts
                .Include(x => x.WalletContractPlans).ThenInclude(x => x.Plan)
                .Include(x => x.WalletContractGuarantors)
                .Include(x => x.WalletContractFinanciers)
                .Include(x => x.WalletContractFacilitators)
                .Include(x => x.WalletContractCustomers)
                .Include(x => x.WalletContractRejectionReasons)
            .Include(x => x.Organization)
            .FirstOrDefaultAsync(x => x.Id == id && (!tenantId.HasValue || x.TenantId == tenantId));
            return contract;
        }

        public async Task<WalletContract> GetByGrantingProcessIdAsync(int grantingProcessId, int tenantId)
        {
            var contract = await _applicationDbContext.WalletContracts
                .Include(x => x.WalletContractPlans).ThenInclude(x => x.Plan)
                .Include(x => x.WalletContractGuarantors)
                .Include(x => x.WalletContractFinanciers)
                .Include(x => x.WalletContractFacilitators)
                .Include(x => x.WalletContractCustomers)
                .Include(x => x.WalletContractRejectionReasons)
                .Include(x => x.Organization)
                .FirstOrDefaultAsync(x => x.GrantingProcessId == grantingProcessId && x.TenantId == tenantId);
            return contract;
        }

        public async Task<WalletContract> GetAsync(int id)
        {
            var contract = await _applicationDbContext.WalletContracts
                .Include(x => x.WalletContractPlans)
                .ThenInclude(x => x.Plan)
                .ThenInclude(x => x.PlanDetails)
                .ThenInclude(x => x.PlanDetailInstallments)
                .FirstOrDefaultAsync(x => x.Id == id);

            return contract;
        }

        public async Task<WalletContract> GetByGrantingProcessIdAsync(int grantingProcessId)
        {
            var contract = await _applicationDbContext.WalletContracts
                .Include(x => x.WalletContractPlans)
                .ThenInclude(x => x.Plan)
                .ThenInclude(x => x.PlanDetails)
                .ThenInclude(x => x.PlanDetailInstallments)
                .Include(x => x.WalletContractPlans)
                .ThenInclude(x => x.Plan).ThenInclude(x => x.WalletConfiguration)
                .FirstOrDefaultAsync(x => x.GrantingProcessId == grantingProcessId);

            return contract;
        }

        public async Task<WalletContractBusinessIdentity> GetWalletContractBusinessIdentityAsync(int walletContractId,
            int businessIdentityId)
        {
            var businessIdentity = await _applicationDbContext.WalletContractBusinessIdentities
                .Include(x => x.BusinessIdentity)
                .FirstOrDefaultAsync(x => x.WalletContractId == walletContractId && x.BusinessIdentityId == businessIdentityId);

            return businessIdentity;
        }

        public async Task<List<WalletContractBusinessIdentity>> GetCustomersWithoutWalletAsync(int walletContractId)
        {
            var customers = await _applicationDbContext.WalletContractBusinessIdentities
                .Where(x => x.WalletContractId == walletContractId && !x.HasWallet)
                .ToListAsync();

            return customers;
        }

        public async Task<List<int>> GetContractsWithCustomerWithoutWalletAsync()
        {
            var walletContractIds = await _applicationDbContext.WalletContractBusinessIdentities
                .Where(x => !x.HasWallet && x.CreatedDateTime > DateTime.Now.AddDays(-1) /*&& !x.WalletContract.IsInProcess*/ )
                .Select(x => x.WalletContractId)
                .Distinct()
                .ToListAsync();

            return walletContractIds;
        }

        public async Task<WalletContractPlan> GetWalletContractPlanForCashWallet(int tenantId)
        {
            var result = await _applicationDbContext.WalletContractPlans
                .Where(c =>
                    c.WalletContract.TenantId == tenantId
                    && c.Plan.WalletConfiguration.WalletTypeId == WalletType.Cash)
                .FirstOrDefaultAsync();

            return result;
        }

        public async Task<bool> ExistsContractByorganizationIdAndPlanIdsAsync(int tenantId, int organizationId, List<int> planIds)
        {
            return await _applicationDbContext.WalletContracts
                            .AnyAsync(x =>
                                x.TenantId == tenantId &&
                                x.OrganizationId == organizationId &&
                                x.Status != WalletContractStatus.Reject &&
                                x.WalletContractPlans.Any(p => planIds.Contains(p.PlanId))
                            );
        }

        public async Task<bool> HasEndorsement(int contractId, int tenantId)
        {
            return await _applicationDbContext.WalletContracts.AnyAsync(x => x.ParentId == contractId && x.TenantId == tenantId && x.Status != WalletContractStatus.Reject);
        }
    }

}
