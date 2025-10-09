using System;
using System.Linq;
using System.Threading;
using Domain.Core.Enums;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate.Dtos;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;

namespace Infrastructure.Data.Repository.EfCore.Repositories;

public sealed class FinancialDocumentRepository(ApplicationDbContext applicationDbContext) : IFinancialDocumentRepository
{
    public async Task AddAsync(FinancialDocument financialDocument)
    {
        await applicationDbContext.FinancialDocuments.AddAsync(financialDocument);
    }

    public async Task<FinancialDocument> GetByIdAsync(long id)
    {
        return await applicationDbContext.FinancialDocuments.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<FinancialDocument>> GetRefundsByParentIdAsync(long parentId)
    {
        return await applicationDbContext.FinancialDocuments.Where(p => p.ParentId == parentId && p.Type == FinancialDocumentType.Refund).ToListAsync();
    }

    public void Update(FinancialDocument financialDocument)
    {
        applicationDbContext.FinancialDocuments.Update(financialDocument);
    }

    public void UpdateRange(List<FinancialDocument> financialDocuments)
    {
        applicationDbContext.FinancialDocuments.UpdateRange(financialDocuments);
    }

    public void UpdatePartial(FinancialDocument financialDocument, string propertyName)
    {
        applicationDbContext.FinancialDocuments.Attach(financialDocument);
        applicationDbContext.FinancialDocuments.Entry(financialDocument).Property(propertyName).IsModified = true;
    }

    public async Task<List<FinancialDocument>> GetAllAsync()
    {
        return await applicationDbContext.FinancialDocuments.ToListAsync();
    }

    public async Task<bool> IsTenantPlatformContractUsedInTransaction(int tenantPlatformContractId)
    {
        return await applicationDbContext.FinancialDocuments
            .AnyAsync(x => x.TenantPlatformContractId == tenantPlatformContractId);
    }

    public async Task<List<int>> GetTenantPlatformContractIdsHasTransaction(List<int> tenantPlatformContractIds)
    {
        var result = await applicationDbContext.FinancialDocuments
            .Where(x => tenantPlatformContractIds.Contains(x.TenantPlatformContractId.Value))
            .Select(x => x.TenantPlatformContractId.Value)
            .ToListAsync();

        return result;
    }

    public async Task<bool> IsTenantMerchantContractUsedInTransaction(int tenantMerchantContractId)
    {
        return await applicationDbContext.FinancialDocuments
            .AnyAsync(x => x.TenantMerchantContractId == tenantMerchantContractId);
    }

    public async Task<List<int>> GetTenantMerchantContractIdsHasTransaction(List<int> tenantMerchantContractIds)
    {
        var result = await applicationDbContext.FinancialDocuments
            .Where(x => tenantMerchantContractIds.Contains(x.TenantMerchantContractId.Value))
            .Select(x => x.TenantMerchantContractId.Value)
            .ToListAsync();

        return result;
    }

    public IQueryable<FinancialDocument> CreateJobFinancialDocumentQuery(DateTime startOfPeriod, DateTime endOfPeriod, IEnumerable<int> contractIds)
    {
        return applicationDbContext.FinancialDocuments
            .Where(p => p.Type == FinancialDocumentType.Refund)
            .Where(p => contractIds.Contains(p.TenantMerchantContractId.Value))
            .Where(p => startOfPeriod <= p.CreatedDateTime && p.CreatedDateTime < endOfPeriod);
    }

    public async Task<FinancialDocumentRange> GetFinancialDocumentRange(IEnumerable<int> contractIds, DateTime? lastBillingDueDate, CancellationToken cancellationToken)
    {
        var query = applicationDbContext.FinancialDocuments.AsNoTracking()
            .Where(p => p.Type == FinancialDocumentType.Refund)
            .Where(p => contractIds.Contains(p.TenantMerchantContractId.Value));

        query = lastBillingDueDate.HasValue ?
            query.Where(p => lastBillingDueDate <= p.CreatedDateTime && p.CreatedDateTime < DateTime.Today) :
            query.Where(p => p.CreatedDateTime < DateTime.Today);

        var found = await query.AnyAsync(cancellationToken);

        if (found == false) return null;

        var minCreatedDateTime = await query.MinAsync(p => p.CreatedDateTime, cancellationToken);

        var maxCreatedDateTime = await query.MaxAsync(p => p.CreatedDateTime, cancellationToken);

        return new FinancialDocumentRange(minCreatedDateTime, maxCreatedDateTime);
    }

    public async Task<Dictionary<ContractIdentifier, List<FinancialDocumentDto>>> GetGroupContractFinancialDocuments(
        IQueryable<FinancialDocument> query, CancellationToken cancellationToken)
    {
        if (query == null) return new Dictionary<ContractIdentifier, List<FinancialDocumentDto>>();

        return await query.AsNoTracking().Select(p => new
        {
            p.Amount,
            p.CreatedDateTime,
            PurchaseCommission = p.Parent.Commission,
            p.TenantMerchantContract.TenantId,
            p.TenantMerchantContract.MerchantId,
            p.TenantMerchantContract.BillingPeriod,
            p.TenantMerchantContract.BillingPeriodType,
            p.TenantMerchantContract.DailyBillingOriginDate,
            p.TenantMerchantContract.CommissionCalculationType
        })
        .GroupBy(p => new ContractIdentifier()
        {
            TenantId = p.TenantId,
            MerchantId = p.MerchantId,
            BillingPeriod = p.BillingPeriod,
            BillingPeriodType = p.BillingPeriodType,
            DailyBillingOriginDate = p.DailyBillingOriginDate,
            CommissionCalculationType = p.CommissionCalculationType
        })
        .ToDictionaryAsync(p => p.Key, p => p.Select(q => new FinancialDocumentDto()
        {
            Amount = q.Amount,
            CreatedDateTime = q.CreatedDateTime,
            PurchaseCommission = q.PurchaseCommission
        }).ToList(), cancellationToken);
    }

    public async Task<IEnumerable<FinancialDocumentDto>> GetFinancialDocumentsInSpecificPeriod(ContractGroup contract,
        DateTime startOfPeriod, DateTime endOfPeriod, CancellationToken cancellationToken)
    {
        return await applicationDbContext.FinancialDocuments
            .Where(p => p.Type == FinancialDocumentType.Refund &&
                        startOfPeriod <= p.CreatedDateTime && p.CreatedDateTime < endOfPeriod &&
                        p.TenantMerchantContract.BillingPeriod == contract.BillingPeriod &&
                        p.TenantMerchantContract.BillingPeriodType == contract.BillingPeriodType &&
                        p.TenantMerchantContract.DailyBillingOriginDate == contract.DailyBillingOriginDate &&
                        p.TenantMerchantContract.CommissionCalculationType == contract.CommissionCalculationType &&
                        p.FromBusinessIdentityId == contract.TenantId && p.ToBusinessIdentityId == contract.MerchantId)
            .Select(p => new FinancialDocumentDto()
            {
                Amount = p.Amount,
                CreatedDateTime = p.CreatedDateTime,
                PurchaseCommission = p.Parent.Commission
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetSumOfRefundTransactionsInSpecificPeriod(ContractGroup contract,
        DateTime startOfPeriod, DateTime endOfPeriod, CancellationToken cancellationToken)
    {
        return await applicationDbContext.FinancialDocuments
            .Where(p => p.Type == FinancialDocumentType.Refund &&
                        startOfPeriod <= p.CreatedDateTime && p.CreatedDateTime < endOfPeriod &&
                        p.TenantMerchantContract.BillingPeriod == contract.BillingPeriod &&
                        p.TenantMerchantContract.BillingPeriodType == contract.BillingPeriodType &&
                        p.TenantMerchantContract.DailyBillingOriginDate == contract.DailyBillingOriginDate &&
                        p.TenantMerchantContract.CommissionCalculationType == contract.CommissionCalculationType &&
                        p.FromBusinessIdentityId == contract.TenantId && p.ToBusinessIdentityId == contract.MerchantId)
            .SumAsync(p => p.Amount, cancellationToken);
    }

    public async Task<decimal> GetSumOfRefundCommissionsInSpecificPeriod(ContractGroup contract,
        DateTime startOfPeriod, DateTime endOfPeriod, CancellationToken cancellationToken)
    {
        return await applicationDbContext.FinancialDocuments
            .Where(p => p.Type == FinancialDocumentType.Refund &&
                        startOfPeriod <= p.CreatedDateTime && p.CreatedDateTime < endOfPeriod &&
                        p.TenantMerchantContract.BillingPeriod == contract.BillingPeriod &&
                        p.TenantMerchantContract.BillingPeriodType == contract.BillingPeriodType &&
                        p.TenantMerchantContract.DailyBillingOriginDate == contract.DailyBillingOriginDate &&
                        p.TenantMerchantContract.CommissionCalculationType == contract.CommissionCalculationType &&
                        p.FromBusinessIdentityId == contract.TenantId && p.ToBusinessIdentityId == contract.MerchantId)
            .SumAsync(p => p.Parent.Commission, cancellationToken) ?? 0;
    }
}