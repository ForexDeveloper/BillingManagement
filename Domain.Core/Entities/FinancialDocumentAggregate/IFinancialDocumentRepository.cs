using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Core.Entities.FinancialDocumentAggregate;

public interface IFinancialDocumentRepository
{
    Task AddAsync(FinancialDocument financialDocument);
    void Update(FinancialDocument financialDocument);
    void UpdateRange(List<FinancialDocument> financialDocuments);
    Task<FinancialDocument> GetByIdAsync(long id);
    Task<List<FinancialDocument>> GetRefundsByParentIdAsync(long parentId);
    Task<List<FinancialDocument>> GetAllAsync();
    Task<bool> IsTenantPlatformContractUsedInTransaction(int tenantPlatformContractId);
    Task<List<int>> GetTenantPlatformContractIdsHasTransaction(List<int> tenantPlatformContractIds);
    Task<bool> IsTenantMerchantContractUsedInTransaction(int tenantMerchantContractId);
    Task<List<int>> GetTenantMerchantContractIdsHasTransaction(List<int> tenantMerchantContractIds);
    IQueryable<FinancialDocument> CreateJobFinancialDocumentQuery(DateTime startOfPeriod, DateTime endOfPeriod, IEnumerable<int> contractIds);
    Task<bool> FindInContractPeriodAsync(IQueryable<FinancialDocument> query, CancellationToken cancellationToken);
    Task<Dictionary<ContractIdentifier, List<FinancialDocument>>> GetGroupContractFinancialDocuments(IQueryable<FinancialDocument> query, CancellationToken cancellationToken);
}
