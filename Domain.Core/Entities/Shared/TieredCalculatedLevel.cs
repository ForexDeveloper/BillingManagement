namespace Domain.Core.Entities.Shared;

public sealed record TieredCalculatedLevel
{
    public int Number { get; set; }

    public decimal Commission { get; set; }

    public decimal TransactionsAmount { get; set; }

    public TieredCommission TieredCommission { get;set; }
}