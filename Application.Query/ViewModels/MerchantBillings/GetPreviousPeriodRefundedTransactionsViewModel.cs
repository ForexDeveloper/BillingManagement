namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetPreviousPeriodRefundedTransactionsViewModel
{
    public long Id { get; set; }

    public decimal Amount { get; set; }
}