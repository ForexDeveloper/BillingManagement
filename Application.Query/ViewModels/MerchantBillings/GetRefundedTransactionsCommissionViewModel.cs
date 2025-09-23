namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetRefundedTransactionsCommissionViewModel
{
    public long Id { get; set; }

    public decimal Amount { get; set; }

    public GetMerchantBillingContractViewModel Contract { get; set; }
}