namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetRefundedTransactionsVm
{
    public long Id { get; set; }

    public int MerchantId { get; set; }

    public int ContractId { get; set; }

    public decimal Amount { get; set; }
}