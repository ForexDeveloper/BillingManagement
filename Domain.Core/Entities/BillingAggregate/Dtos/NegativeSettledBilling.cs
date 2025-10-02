using Domain.Core.Entities.MerchantBillingAggregate;

namespace Domain.Core.Entities.BillingAggregate.Dtos;

public sealed record NegativeSettledBilling
{
    public MerchantBilling Billing { get; set; }

    public int ActiveContractId { get; set; }
}