using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public sealed class TenantMerchantContractRepository(ApplicationDbContext applicationDbContext)
    : Repository<TenantMerchantContract, int>(applicationDbContext), ITenantMerchantContractRepository
{
    private readonly ApplicationDbContext _applicationDbContext = applicationDbContext;

    public async Task AddAsync(TenantMerchantContract tenantMerchantContract)
    {
        await _applicationDbContext.TenantMerchantContracts.AddAsync(tenantMerchantContract);
    }

    public async Task<TenantMerchantContract> GetAsync(int id)
    {
        var contract = await _applicationDbContext.TenantMerchantContracts
            .Include(x => x.Tenant)
            .Include(x => x.Merchant)
            .FirstOrDefaultAsync(x => x.Id == id);

        return contract;
    }

    public async Task<bool> IsContractBelongToTenantAsync(int id, int tenantId)
    {
        var result = await _applicationDbContext.TenantMerchantContracts.AnyAsync(x => x.Id == id && x.TenantId == tenantId);

        return result;
    }

    public async Task<bool> IsExistsActiveContractAsync(int tenantId, int merchantId, int? contractId = null)
    {
        return await _applicationDbContext.TenantMerchantContracts.AnyAsync(x =>
            x.TenantId == tenantId && x.MerchantId == merchantId && x.Status == true && x.EndDate >= System.DateTime.Now && (!contractId.HasValue || x.Id != contractId));
    }

    public async Task<bool> IsDuplicatedContractNumberAsync(string contractNumber)
    {
        return await _applicationDbContext.TenantMerchantContracts.AnyAsync(x =>
            x.ContractNumber == contractNumber.Trim() && !x.IsDeleted);
    }

    public async Task<TenantMerchantContract> GetActiveContractAsync(int tenantId, int merchantId)
    {
        //TODO  x.EndDate >= DateTime.Now && DateTime.Now >= x.StartDate 

        return await _applicationDbContext.TenantMerchantContracts.OrderByDescending(p => p.CreatedDateTime)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.MerchantId == merchantId && x.Status);

        return await _applicationDbContext.TenantMerchantContracts
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.MerchantId == merchantId && x.Status == true && x.EndDate >= DateTime.Now);
    }

    public async Task<bool> HasEndorsement(int contractId, int tenantId)
    {
        return await _applicationDbContext.TenantMerchantContracts.AnyAsync(x => x.ParentId == contractId && x.TenantId == tenantId);
    }

    public async Task<List<int>> GetContractIdsHasEndorsement(List<int> contractIds)
    {
        var result = await _applicationDbContext.TenantMerchantContracts
        .Where(x => contractIds.Contains(x.ParentId.Value))
        .Select(x => x.ParentId.Value)
        .ToListAsync();

        return result;
    }

    public async Task<List<ContractGroup>> GetAllGroupContractAsync(CancellationToken cancellationToken)
    {
        var contracts = await _applicationDbContext.TenantMerchantContracts.AsNoTracking().Select(p => new
        {
            p.Id,
            p.Status,
            p.TenantId,
            p.MerchantId,
            p.BillingPeriod,
            p.CreatedDateTime,
            p.BillingPeriodType,
            p.TieredCommissions,
            p.DailyBillingOriginDate,
            p.CommissionReferenceTypes,
            p.CommissionCalculationType,
            p.PeriodMinCommissionAmount,
            p.PeriodMaxCommissionAmount
        })
        .ToListAsync(cancellationToken);

        return contracts.GroupBy(p => new ContractIdentifier()
        {
            TenantId = p.TenantId,
            MerchantId = p.MerchantId,
            BillingPeriod = p.BillingPeriod,
            BillingPeriodType = p.BillingPeriodType,
            CommissionCalculationType = p.CommissionCalculationType,
            DailyBillingOriginDate = p.DailyBillingOriginDate?.Date
        })
        .Select(p => new ContractGroup()
        {
            TenantId = p.Key.TenantId,
            MerchantId = p.Key.MerchantId,
            BillingPeriod = p.Key.BillingPeriod,
            BillingPeriodType = p.Key.BillingPeriodType,
            DailyBillingOriginDate = p.Key.DailyBillingOriginDate,
            CommissionCalculationType = p.Key.CommissionCalculationType,
            ContractIds = p.OrderByDescending(q => q.CreatedDateTime).Select(q => q.Id).ToList(),

            MainContractId = p.Any(q => q.Status)
                ? p.First(q => q.Status).Id
                : p.OrderByDescending(q => q.CreatedDateTime).First().Id,

            Status = p.Any(q => q.Status)
                ? p.First(q => q.Status).Status
                : p.OrderByDescending(q => q.CreatedDateTime).First().Status,

            TieredCommissions = p.Any(q => q.Status)
                 ? p.First(q => q.Status).TieredCommissions
                 : p.OrderByDescending(q => q.CreatedDateTime).First().TieredCommissions,

            CommissionReferenceTypes = p.Any(q => q.Status)
                 ? p.First(q => q.Status).CommissionReferenceTypes
                 : p.OrderByDescending(q => q.CreatedDateTime).First().CommissionReferenceTypes,

            PeriodMinCommissionAmount = p.Any(q => q.Status)
                 ? p.First(q => q.Status).PeriodMinCommissionAmount
                 : p.OrderByDescending(q => q.CreatedDateTime).First().PeriodMinCommissionAmount,

            PeriodMaxCommissionAmount = p.Any(q => q.Status)
                 ? p.First(q => q.Status).PeriodMaxCommissionAmount
                 : p.OrderByDescending(q => q.CreatedDateTime).First().PeriodMaxCommissionAmount
        })
        .OrderBy(p => p.Status)
        .ToList();
    }
}