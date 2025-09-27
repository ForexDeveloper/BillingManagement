using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;
using Domain.Core.Enums;
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

    Task<bool> ExecuteQueryAnyAsync(IQueryable<FinancialDocument> query, CancellationToken cancellationToken);

    Task<decimal> GetPeriodTotalTransactionsAmount(int tenantId, int merchantId, DateTime startOfPeriod, DateTime endOfPeriod, CancellationToken cancellationToken);

    Task<decimal> GetSumOfTransactionsOfCurrentPeriod(int tenantId, int merchantId, int billingPeriod,
        TimeInterval billingPeriodType, DateTime? dailyBillingOriginDate,
        CommissionCalculationType commissionCalculationType, DateTime startOfPeriod, DateTime endOfPeriod);

    Task<decimal> GetSumOfTransactionsOfCurrentPeriod(TenantMerchantContract contract, DateTime startOfPeriod, DateTime endOfPeriod);

    Task<Dictionary<ContractIdentifier, List<FinancialDocument>>> GetGroupContractFinancialDocuments(IQueryable<FinancialDocument> query, CancellationToken cancellationToken);
}