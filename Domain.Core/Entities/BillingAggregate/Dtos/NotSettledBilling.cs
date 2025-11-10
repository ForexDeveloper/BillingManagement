using Domain.Core.Entities.MerchantBillingAggregate;

namespace Domain.Core.Entities.BillingAggregate.Dtos;

public sealed record NotSettledBilling
{
    public int ActiveContractId { get; set; }

    public decimal PayableAmount { get; set; }

    public MerchantBilling Billing { get; set; }
}