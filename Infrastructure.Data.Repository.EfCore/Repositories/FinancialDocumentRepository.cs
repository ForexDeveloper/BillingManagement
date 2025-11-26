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

    public void Update(FinancialDocument financialDocument)
    {
        applicationDbContext.FinancialDocuments.Update(financialDocument);
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

    public async Task<decimal> GetPurchaseCommission(long id)
    {
        return await applicationDbContext.FinancialDocuments
            .Where(p => p.Id == id).Select(p => p.Commission).FirstOrDefaultAsync();
    }

    public async Task<decimal> GetPurchaseTransaction(long id)
    {
        return await applicationDbContext.FinancialDocuments
             .Where(p => p.Id == id).Select(p => p.Amount).FirstOrDefaultAsync();
    }

    public async Task<decimal> GetSumOfRefundTransactions(long id)
    {
        return await applicationDbContext.FinancialDocuments
            .Where(p => p.Type == FinancialDocumentType.Refund)
            .Where(p => p.ParentId == id).SumAsync(p => p.Amount);
    }

    public async Task<decimal> GetSumOfRefundCommissions(long id)
    {
        return await applicationDbContext.FinancialDocuments
            .Where(p => p.Type == FinancialDocumentType.Refund)
            .Where(p => p.ParentId == id).SumAsync(p => p.Commission);
    }

    public async Task<FinancialDocumentRange> GetFinancialDocumentRange(IEnumerable<int> contractIds,
        DateTime? lastBillingDueDate, DateTime jobCreatedDateTime, CancellationToken cancellationToken)
    {
        var query = applicationDbContext.FinancialDocuments.AsNoTracking()
            .Where(p => p.Type == FinancialDocumentType.Refund)
            .Where(p => contractIds.Contains(p.TenantMerchantContractId.Value));

        query = lastBillingDueDate.HasValue ?
            query.Where(p => lastBillingDueDate <= p.CreatedDateTime && p.CreatedDateTime < DateTime.Today) :
            query.Where(p => jobCreatedDateTime <= p.CreatedDateTime && p.CreatedDateTime < DateTime.Today);

        var found = await query.AnyAsync(cancellationToken);

        if (found == false) return null;

        var minCreatedDateTime = await query.MinAsync(p => p.CreatedDateTime, cancellationToken);

        var maxCreatedDateTime = await query.MaxAsync(p => p.CreatedDateTime, cancellationToken);

        return new FinancialDocumentRange(minCreatedDateTime, maxCreatedDateTime);
    }

    public async Task<IEnumerable<FinancialDocumentDto>> GetFinancialDocumentsInSpecificPeriod(IEnumerable<int> contractIds,
        DateTime startOfPeriod, DateTime endOfPeriod, CancellationToken cancellationToken)
    {
        return await applicationDbContext.FinancialDocuments
            .Where(p => p.Type == FinancialDocumentType.Refund)
            .Where(p => contractIds.Contains(p.TenantMerchantContractId.Value))
            .Where(p => startOfPeriod <= p.CreatedDateTime && p.CreatedDateTime < endOfPeriod)
            .Select(p => new FinancialDocumentDto()
            {
                Amount = p.Amount,
                CreatedDateTime = p.CreatedDateTime,
                PurchaseCommission = p.Parent.Commission
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetSumOfRefundTransactionsInSpecificPeriod(IEnumerable<int> contractIds,
        DateTime startOfPeriod, DateTime endOfPeriod, CancellationToken cancellationToken)
    {
        return await applicationDbContext.FinancialDocuments
            .Where(p => p.Type == FinancialDocumentType.Refund)
            .Where(p => contractIds.Contains(p.TenantMerchantContractId.Value))
            .Where(p => startOfPeriod <= p.CreatedDateTime && p.CreatedDateTime < endOfPeriod)
            .SumAsync(p => p.Amount, cancellationToken);
    }

    public async Task<decimal> GetSumOfRefundCommissionsInSpecificPeriod(IEnumerable<int> contractIds,
        DateTime startOfPeriod, DateTime endOfPeriod, CancellationToken cancellationToken)
    {
        return await applicationDbContext.FinancialDocuments
            .Where(p => p.Type == FinancialDocumentType.Refund)
            .Where(p => contractIds.Contains(p.TenantMerchantContractId.Value))
            .Where(p => startOfPeriod <= p.CreatedDateTime && p.CreatedDateTime < endOfPeriod)
            .SumAsync(p => p.Commission, cancellationToken);
    }
}