using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Core.Entities.FinancialDocumentAggregate;

public interface IFinancialDocumentRepository
{
    Task AddAsync(FinancialDocument financialDocument);
    void Update(FinancialDocument financialDocument);
    void UpdateRange(List<FinancialDocument> financialDocuments);
    Task<FinancialDocument> GetByIdAsync(long id);
    Task<List<FinancialDocument>> GetRefundsByParentIdAsync(long parentId);
    Task<FinancialDocument> GetAsync(long paymentId);
    Task<List<FinancialDocument>> GetAllAsync();
    Task<bool> IsTenantPlatformContractUsedInTransaction(int tenantPlatformContractId);
    Task<List<int>> GetTenantPlatformContractIdsHasTransaction(List<int> tenantPlatformContractIds);
    Task<bool> IsTenantMerchantContractUsedInTransaction(int tenantMerchantContractId);
    Task<List<int>> GetTenantMerchantContractIdsHasTransaction(List<int> tenantMerchantContractIds);
    Task<bool> IsWalletContractUsedInTransaction(int walletContractId);
    Task<List<int>> GetWalletContractIdsHasTransaction(List<int> walletContractIds);
}
