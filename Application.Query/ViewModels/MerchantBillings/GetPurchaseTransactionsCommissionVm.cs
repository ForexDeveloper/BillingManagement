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

    public List<TieredCommissionLevel> TieredCommissionLevels { get; set; }

    public IEnumerable<GetMerchantBillingContractVm> Contracts { get; set; }
}

public sealed record TieredCommissionLevel(int Number, decimal Amount, decimal TransactionsAmount)
{
    public int Number { get; set; } = Number;

    public decimal Amount { get; set; } = Amount;

    public decimal TransactionsAmount { get; set; } = TransactionsAmount;
}