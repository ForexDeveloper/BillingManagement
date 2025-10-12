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

    Task<GetAdditionsViewModel> GetAdditionsAsync(GetAdditionsQuery query);

    Task<GetDeductionsViewModel> GetDeductionsAsync(GetDeductionsQuery query);

    Task<GetRefundedTransactionsCommissionViewModel> GetRefundedPurchasesCommissionAsync(GetRefundedPurchasesCommissionQuery query);

    Task<GetPreviousPeriodRefundedTransactionsViewModel> GetPreviousPeriodRefundedPurchasesAsync(GetPreviousPeriodRefundedTransactionsQuery query);

    Task<GetCurrentPeriodFinalCommissionViewModel> GetCurrentPeriodFinalCommissionAsync(GetCurrentPeriodFinalCommissionQuery query);

    Task<GetMerchantBillingQueryModel> GetBillingByIdAsync(long id, int tenantId);
}