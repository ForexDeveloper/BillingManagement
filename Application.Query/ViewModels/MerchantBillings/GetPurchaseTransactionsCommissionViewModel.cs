using System.Collections.Generic;

namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetPurchaseTransactionsCommissionViewModel
{
    public long Id { get; set; }

    public decimal Amount { get; set; }

    public string Message { get; set; }

    public int TransactionCount { get; set; }

    public decimal CalculatedCommission { get; set; }

    public decimal PurchaseTransactionsCommission { get; set; }

    public IEnumerable<GetMerchantBillingContractViewModel> Contracts { get; set; }
}