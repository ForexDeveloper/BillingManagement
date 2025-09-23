using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;
using Domain.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public class FinancialDocumentRepository : IFinancialDocumentRepository
{
    private readonly ApplicationDbContext _applicationDbContext;

    public FinancialDocumentRepository(ApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task AddAsync(FinancialDocument financialDocument)
    {
        await _applicationDbContext.FinancialDocuments.AddAsync(financialDocument);
    }

    public async Task<FinancialDocument> GetByIdAsync(long id)
    {
        return await _applicationDbContext.FinancialDocuments.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<FinancialDocument>> GetRefundsByParentIdAsync(long parentId)
    {
        return await _applicationDbContext.FinancialDocuments.Where(p => p.ParentId == parentId && p.Type == FinancialDocumentType.Refund).ToListAsync();
    }

    public void Update(FinancialDocument financialDocument)
    {
        _applicationDbContext.FinancialDocuments.Update(financialDocument);
    }

    public void UpdateRange(List<FinancialDocument> financialDocuments)
    {
        _applicationDbContext.FinancialDocuments.UpdateRange(financialDocuments);
    }

    public async Task<List<FinancialDocument>> GetAllAsync()
    {
        return await _applicationDbContext.FinancialDocuments.ToListAsync();
    }

    public async Task<bool> IsTenantPlatformContractUsedInTransaction(int tenantPlatformContractId)
    {
        return await _applicationDbContext.FinancialDocuments
            .AnyAsync(x => x.TenantPlatformContractId == tenantPlatformContractId);
    }

    public async Task<List<int>> GetTenantPlatformContractIdsHasTransaction(List<int> tenantPlatformContractIds)
    {
        var result = await _applicationDbContext.FinancialDocuments
            .Where(x => tenantPlatformContractIds.Contains(x.TenantPlatformContractId.Value))
            .Select(x => x.TenantPlatformContractId.Value)
            .ToListAsync();

        return result;
    }

    public async Task<bool> IsTenantMerchantContractUsedInTransaction(int tenantMerchantContractId)
    {
        return await _applicationDbContext.FinancialDocuments
            .AnyAsync(x => x.TenantMerchantContractId == tenantMerchantContractId);
    }

    public async Task<List<int>> GetTenantMerchantContractIdsHasTransaction(List<int> tenantMerchantContractIds)
    {
        var result = await _applicationDbContext.FinancialDocuments
            .Where(x => tenantMerchantContractIds.Contains(x.TenantMerchantContractId.Value))
            .Select(x => x.TenantMerchantContractId.Value)
            .ToListAsync();

        return result;
    }

    public IQueryable<FinancialDocument> CreateJobFinancialDocumentQuery(DateTime startOfPeriod, DateTime endOfPeriod, IEnumerable<int> contractIds)
    {
        return _applicationDbContext.FinancialDocuments
            .Where(p => startOfPeriod <= p.CreatedDateTime && p.CreatedDateTime < endOfPeriod)
            .Where(p => contractIds.Contains(p.TenantMerchantContractId.Value));
    }

    public async Task<bool> FindInContractPeriodAsync(IQueryable<FinancialDocument> query, CancellationToken cancellationToken)
    {
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<Dictionary<ContractIdentifier, List<FinancialDocument>>> GetGroupContractFinancialDocuments(IQueryable<FinancialDocument> query, CancellationToken cancellationToken)
    {
        return await query.Select(p => new
        {
            FinancialDocument = p,
            p.TenantMerchantContract.TenantId,
            p.TenantMerchantContract.MerchantId,
            p.TenantMerchantContract.BillingPeriod,
            p.TenantMerchantContract.BillingPeriodType,
            p.TenantMerchantContract.DailyBillingOriginDate
        })
        .GroupBy(p => new ContractIdentifier()
        {
            TenantId = p.TenantId,
            MerchantId = p.MerchantId,
            BillingPeriod = p.BillingPeriod,
            BillingPeriodType = p.BillingPeriodType,
            BillingDailyOriginDate = p.DailyBillingOriginDate
        })
        .ToDictionaryAsync(p => p.Key, p => p.Select(q => q.FinancialDocument).ToList(), cancellationToken);
    }

}
