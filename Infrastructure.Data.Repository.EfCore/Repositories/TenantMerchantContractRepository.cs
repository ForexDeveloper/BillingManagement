using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate.ValueObjects;

namespace Infrastructure.Data.Repository.EfCore.Repositories
{
    public class TenantMerchantContractRepository(ApplicationDbContext applicationDbContext)
        : ITenantMerchantContractRepository
    {
        private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;

        public async Task AddAsync(TenantMerchantContract tenantMerchantContract)
        {
            await applicationDbContext.TenantMerchantContracts.AddAsync(tenantMerchantContract);
        }

        public void Update(TenantMerchantContract tenantMerchantContract)
        {
            applicationDbContext.TenantMerchantContracts.Update(tenantMerchantContract);
        }

        public async Task<TenantMerchantContract> GetAsync(int id)
        {
            var contract = await applicationDbContext.TenantMerchantContracts
                .Include(x => x.Tenant)
                .Include(x => x.Merchant)
                .FirstOrDefaultAsync(x => x.Id == id);

            return contract;
        }

        public async Task<bool> IsContractBelongToTenantAsync(int id, int tenantId)
        {
            var result = await applicationDbContext.TenantMerchantContracts.AnyAsync(x => x.Id == id && x.TenantId == tenantId);

            return result;
        }

        public async Task<bool> IsExistsActiveContractAsync(int tenantId, int merchantId, int? contractId = null)
        {
            return await applicationDbContext.TenantMerchantContracts.AnyAsync(x =>
                x.TenantId == tenantId && x.MerchantId == merchantId && x.Status == true && x.EndDate >= System.DateTime.Now && (!contractId.HasValue || x.Id != contractId));
        }

        public async Task<bool> IsDuplicatedContractNumberAsync(string contractNumber)
        {
            return await applicationDbContext.TenantMerchantContracts.AnyAsync(x =>
                x.ContractNumber == contractNumber.Trim() && !x.IsDeleted);
        }

        public async Task<TenantMerchantContract> GetActiveContractAsync(int tenantId, int merchantId)
        {
            //TODO  x.EndDate >= DateTime.Now && DateTime.Now >= x.StartDate 

            return await applicationDbContext.TenantMerchantContracts
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.MerchantId == merchantId && x.Status == true && x.EndDate >= DateTime.Now);
        }

        public async Task<bool> HasEndorsement(int contractId, int tenantId)
        {
            return await applicationDbContext.TenantMerchantContracts.AnyAsync(x => x.ParentId == contractId && x.TenantId == tenantId);
        }

        public async Task<List<int>> GetContractIdsHasEndorsement(List<int> contractIds)
        {
            var result = await applicationDbContext.TenantMerchantContracts
            .Where(x => contractIds.Contains(x.ParentId.Value))
            .Select(x => x.ParentId.Value)
            .ToListAsync();

            return result;
        }

        public async Task<List<NoInstallmentContract>> GetNoInstallmentContracts(CancellationToken cancellationToken)
        {
            //TODO DateTime Check if we want to have two active contracts

            var today = DateTime.Today;

            var yesterday = DateTime.Today.AddDays(-1);

            return await applicationDbContext.TenantMerchantContracts
                .Where(p => p.Status && !p.Installments.Any(q => yesterday <= q.DueDate && q.DueDate < today))
                .Select(p => new NoInstallmentContract
                {
                    Id = p.Id,
                    TenantId = p.TenantId,
                    MerchantId = p.MerchantId,
                    BillingPeriod = p.BillingPeriod,
                    BillingPeriodType = p.BillingPeriodType,
                    DailyBillingOriginDate = p.DailyBillingOriginDate,
                    PeriodMaxCommissionAmount = p.PeriodMaxCommissionAmount,
                    PeriodMinCommissionAmount = p.PeriodMinCommissionAmount
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}