using Application.Query.ViewModels.Billings;

namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetMerchantBillingVm : GetBillingVm
{
    public decimal PurchaseTransactionsAmount { get; set; }

    public decimal RefundedTransactionsAmount { get; set; }

    public decimal RefundedTransactionsCommission { get; set; }

    public decimal PurchaseTransactionsCommission { get; set; }
}