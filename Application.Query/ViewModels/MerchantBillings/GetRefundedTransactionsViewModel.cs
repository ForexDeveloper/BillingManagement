namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetRefundedTransactionsViewModel
{
    public long Id { get; set; }

    public decimal Amount { get; set; }
}