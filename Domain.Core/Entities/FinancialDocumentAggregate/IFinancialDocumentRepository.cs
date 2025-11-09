using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Domain.Core.Entities.FinancialDocumentAggregate.Dtos;

namespace Domain.Core.Entities.FinancialDocumentAggregate;

public interface IFinancialDocumentRepository
{
    Task AddAsync(FinancialDocument financialDocument);

    void Update(FinancialDocument financialDocument);

    Task<FinancialDocument> GetByIdAsync(long id);

    Task<bool> IsTenantPlatformContractUsedInTransaction(int tenantPlatformContractId);

    Task<List<int>> GetTenantPlatformContractIdsHasTransaction(List<int> tenantPlatformContractIds);

    Task<bool> IsTenantMerchantContractUsedInTransaction(int tenantMerchantContractId);

    Task<List<int>> GetTenantMerchantContractIdsHasTransaction(List<int> tenantMerchantContractIds);

    Task<FinancialDocumentRange?> GetFinancialDocumentRange(IEnumerable<int> contractIds, DateTime? lastBillingDueDate,
        DateTime jobCreatedDateTime, CancellationToken cancellationToken);

    Task<IEnumerable<FinancialDocumentDto>> GetFinancialDocumentsInSpecificPeriod(IEnumerable<int> contractIds,
        DateTime startOfPeriod, DateTime endOfPeriod, CancellationToken cancellationToken);

    Task<decimal> GetSumOfRefundTransactionsInSpecificPeriod(IEnumerable<int> contractIds, DateTime startOfPeriod,
        DateTime endOfPeriod, CancellationToken cancellationToken);

    Task<decimal> GetSumOfRefundCommissionsInSpecificPeriod(IEnumerable<int> contractIds, DateTime startOfPeriod,
        DateTime endOfPeriod, CancellationToken cancellationToken);
}