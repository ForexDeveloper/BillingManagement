using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Domain.Core.Entities.FinancialDocumentAggregate.Dtos;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;

namespace Domain.Core.Entities.FinancialDocumentAggregate;

public interface IFinancialDocumentRepository
{
    Task AddAsync(FinancialDocument financialDocument);

    void Update(FinancialDocument financialDocument);

    void UpdateRange(List<FinancialDocument> financialDocuments);

    void UpdatePartial(FinancialDocument financialDocument, string propertyName);

    Task<FinancialDocument> GetByIdAsync(long id);

    Task<List<FinancialDocument>> GetRefundsByParentIdAsync(long parentId);

    Task<List<FinancialDocument>> GetAllAsync();

    Task<bool> IsTenantPlatformContractUsedInTransaction(int tenantPlatformContractId);

    Task<List<int>> GetTenantPlatformContractIdsHasTransaction(List<int> tenantPlatformContractIds);

    Task<bool> IsTenantMerchantContractUsedInTransaction(int tenantMerchantContractId);

    Task<List<int>> GetTenantMerchantContractIdsHasTransaction(List<int> tenantMerchantContractIds);

    IQueryable<FinancialDocument> CreateJobFinancialDocumentQuery(DateTime startOfPeriod, DateTime endOfPeriod, IEnumerable<int> contractIds);

    Task<FinancialDocumentRange?> GetFinancialDocumentRange(IEnumerable<int> contractIds, DateTime? lastBillingDueDate,
        DateTime jobCreatedDateTime, CancellationToken cancellationToken);

    Task<IEnumerable<FinancialDocumentDto>> GetFinancialDocumentsInSpecificPeriod(IEnumerable<int> contractIds,
        DateTime startOfPeriod, DateTime endOfPeriod, CancellationToken cancellationToken);

    Task<decimal> GetSumOfRefundTransactionsInSpecificPeriod(IEnumerable<int> contractIds, DateTime startOfPeriod,
        DateTime endOfPeriod, CancellationToken cancellationToken);

    Task<decimal> GetSumOfRefundCommissionsInSpecificPeriod(IEnumerable<int> contractIds, DateTime startOfPeriod,
        DateTime endOfPeriod, CancellationToken cancellationToken);
}