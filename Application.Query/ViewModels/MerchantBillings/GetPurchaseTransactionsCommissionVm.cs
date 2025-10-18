using System.Collections.Generic;

namespace Application.Query.ViewModels.MerchantBillings;

public sealed record GetPurchaseTransactionsCommissionVm
{
    public long Id { get; set; }

    public string Message { get; set; }

    public int MainContractId { get; set; }

    public decimal FinalAmount { get; set; }

    public int TransactionsCount { get; set; }

    public decimal CalculatedAmount { get; set; }

    public decimal TransactionsAmount { get; set; }

    public IEnumerable<TieredCalculatedLevelVm> TieredCalculatedLevels { get; set; }

    public IEnumerable<GetMerchantBillingContractVm> Contracts { get; set; }
}

public sealed record TieredCalculatedLevelVm
{
    public int Number { get; set; }

    public decimal Commission { get; set; }

    public decimal TransactionsAmount { get; set; }
}