using System.Threading.Tasks;
using Application.Query.ViewModels.Billings;
using Application.Query.Queries.MerchantBilling;
using Application.Query.ViewModels.MerchantBillings;
using Application.Query.QueryModels.MerchantBillings;

namespace Application.Query.ReadOnlyRepositoryContracts;

public interface IMerchantBillingReadOnlyRepository
{
    Task<GetBillingsViewModel> GetBillingsAsync(GetMerchantBillingsQuery query);

    Task<GetMerchantBillingVm> GetBillingAsync(GetMerchantBillingQuery query);

    Task<GetPreviousDebitVm> GetPreviousDebitAsync(GetPreviousDebitQuery query);

    Task<GetPreviousCreditVm> GetPreviousCreditAsync(GetPreviousCreditQuery query);

    Task<GetAdditionsVm> GetAdditionsAsync(GetAdditionsQuery query);

    Task<GetDeductionsVm> GetDeductionsAsync(GetDeductionsQuery query);

    Task<GetPurchaseTransactionsVm> GetPurchaseTransactionsAsync(GetPurchaseTransactionsQuery query);

    Task<GetRefundedTransactionsVm> GetRefundedTransactionsAsync(GetRefundedTransactionsQuery query);

    Task<GetPurchaseTransactionsCommissionVm> GetPurchaseTransactionsCommissionAsync(GetPurchaseTransactionsCommissionQuery query);

    Task<GetRefundedTransactionsCommissionVm> GetRefundedTransactionsCommissionAsync(GetRefundedTransactionsCommissionQuery query);

    Task<GetMerchantBillingQueryModel> GetBillingByIdAsync(long id, int tenantId);
}