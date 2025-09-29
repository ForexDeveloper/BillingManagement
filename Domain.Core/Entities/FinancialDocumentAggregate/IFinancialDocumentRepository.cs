using System;
using System.Linq;
using System.Threading;
using Domain.Core.Enums;
using System.Threading.Tasks;
using System.Collections.Generic;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;

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
        BmCommissionCalculationType commissionCalculationType, DateTime startOfPeriod, DateTime endOfPeriod);

    Task<decimal> GetSumOfTransactionsOfCurrentPeriod(TenantMerchantContract contract, DateTime startOfPeriod, DateTime endOfPeriod);

    Task<Dictionary<ContractIdentifier, List<FinancialDocumentDto>>> GetGroupContractFinancialDocuments(IQueryable<FinancialDocument> query, CancellationToken cancellationToken);
}

public sealed record FinancialDocumentDto
{
    public long Id { get; set; }

    public decimal Amount { get; set; }

    public FinancialDocumentType Type { get; set; }

    public decimal? PurchaseCommission { get; set; }

    public DateTime CreatedDateTime { get; set; }
}