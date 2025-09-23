using Application.Query.ViewModels.Billings;

namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetMerchantBillingViewModel : GetBillingViewModel
{
    public decimal Additions { get; set; }

    public decimal Deductions { get; set; }

    public decimal RefundedPurchasesCommission { get; set; }

    public decimal CurrentPeriodFinalCommission { get; set; }

    public decimal CurrentPeriodPurchaseTransactions { get; set; }

    public decimal PreviousPeriodRefundedPurchases { get; set; }
}