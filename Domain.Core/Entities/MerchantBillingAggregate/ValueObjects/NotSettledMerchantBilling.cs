using System.Collections.Generic;

namespace Domain.Core.Entities.MerchantBillingAggregate.ValueObjects;

public sealed record NotSettledMerchantBilling
{
    public decimal SumOfPayments { get; set; }

    public MerchantBilling Billing { get; set; }

    public IEnumerable<int> ContractIds { get; set; }

    public decimal CalculateDebitAmount()
    {
        return Billing.Amount + Billing.PreviousDebitAmount - SumOfPayments;
    }
}