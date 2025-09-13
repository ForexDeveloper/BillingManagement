using Application.Query.Queries;
using Application.Query.QueryModels;
using Application.Query.ViewModels.Tenants;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts
{
    public interface IFinancialDocumentReadOnlyRepository
    {
        Task<CustomerFinancialDocumentsQueryModel> GetCustomerFinancialDocumentsAsync(
            GetCustomerFinancialDocumentsQuery query, CancellationToken cancellationToken);
        Task<CustomerFinancialDocumentDetailQueryModel> GetCustomerFinancialDocumentAsync(GetCustomerFinancialDocumentQuery query, CancellationToken cancellationToken);
        Task<GetPurchasesListVm> GetPurchasesListAsync(GetPurchasesListQuery filter, CancellationToken cancellationToken);
        Task<GetPurchaseVm> GetPurchaseAsync(long financialDocumentId, CancellationToken cancellationToken);
        Task<GetMerchantPurchasesQueryModel> GetPurchasesByMerchantIdAsync(GetMerchantPurchasesQuery query);
        Task<RefundFinancialDocumentQueryModel> GetFinanialDocumentRefundDetailByIdAsync(long id, int merchantId, int? merchantBranchId = null, int? tenantId = null);
    }
}
