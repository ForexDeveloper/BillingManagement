using Application.Query.Queries.MerchantBilling;
using Application.Query.QueryModels.MerchantBillings;
using Application.Query.ViewModels.Billings;
using Application.Query.ViewModels.MerchantBillings;
using System.Threading.Tasks;

namespace Application.Query.ReadOnlyRepositoryContracts;

public interface IMerchantBillingReadOnlyRepository
{
    Task<GetBillingsViewModel> GetBillingsAsync(GetMerchantBillingsQuery query);

    Task<GetMerchantBillingViewModel> GetBillingAsync(GetMerchantBillingQuery query);

    Task<GetPreviousDebitViewModel> GetPreviousDebitAsync(GetPreviousDebitQuery query);

    Task<GetPreviousCreditViewModel> GetPreviousCreditAsync(GetPreviousCreditQuery query);

    Task<GetAdditionsViewModel> GetAdditionsAsync(GetAdditionsQuery query);

    Task<GetDeductionsViewModel> GetDeductionsAsync(GetDeductionsQuery query);

    Task<GetPurchaseTransactionsViewModel> GetPurchaseTransactionsAsync(GetPurchaseTransactionsQuery query);

    Task<GetRefundedTransactionsViewModel> GetRefundedTransactionsAsync(GetRefundedTransactionsQuery query);

    Task<GetPurchaseTransactionsCommissionViewModel> GetPurchaseTransactionsCommissionAsync(GetPurchaseTransactionsCommissionQuery query);

    Task<GetRefundedTransactionsCommissionViewModel> GetRefundedTransactionsCommissionAsync(GetRefundedTransactionsCommissionQuery query);
    Task<GetMerchantBillingQueryModel> GetBillingByIdAsync(long id, int tenantId);
}