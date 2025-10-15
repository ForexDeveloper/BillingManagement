using System.Collections.Generic;

namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetPurchaseTransactionsCommissionVm
{
    public long Id { get; set; }

    public string Message { get; set; }

    public decimal FinalAmount { get; set; }

    public int TransactionsCount { get; set; }

    public decimal CalculatedAmount { get; set; }

    public decimal TransactionsAmount { get; set; }

    public IEnumerable<GetMerchantBillingContractVm> Contracts { get; set; }
}