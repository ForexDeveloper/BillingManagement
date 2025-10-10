using System.Threading.Tasks;
using Application.Query.ViewModels.Billings;
using Application.Query.Queries.MerchantBilling;
using Application.Query.ViewModels.MerchantBillings;

namespace Application.Query.ReadOnlyRepositoryContracts;

public interface IMerchantBillingReadOnlyRepository
{
    Task<GetBillingsViewModel> GetBillingsAsync(GetMerchantBillingsQuery query);

    Task<GetMerchantBillingViewModel> GetBillingAsync(GetMerchantBillingQuery query);

    Task<GetPreviousDebitViewModel> GetPreviousDebitAsync(GetPreviousDebitQuery query);

    Task<GetAdditionsViewModel> GetAdditionsAsync(GetAdditionsQuery query);

    Task<GetDeductionsViewModel> GetDeductionsAsync(GetDeductionsQuery query);

    Task<GetRefundedTransactionsCommissionViewModel> GetRefundedPurchasesCommissionAsync(GetRefundedTransactionsCommissionQuery query);

    Task<GetRefundedTransactionsViewModel> GetPreviousPeriodRefundedPurchasesAsync(GetRefundedTransactionsQuery query);

    Task<GetPurchaseTransactionsCommissionViewModel> GetPurchaseTransactionsCommissionAsync(GetPurchaseTransactionsCommissionQuery query);
}