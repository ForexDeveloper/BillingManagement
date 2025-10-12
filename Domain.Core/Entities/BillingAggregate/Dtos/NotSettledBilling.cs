using Domain.Core.Entities.MerchantBillingAggregate;

namespace Domain.Core.Entities.BillingAggregate.Dtos;

public sealed record NotSettledBilling
{
    public decimal PaidAmount { get; set; }

    public int ActiveContractId { get; set; }

    public MerchantBilling Billing { get; set; }

    public decimal CalculatePayableAmount()
    {
        return Billing.Amount - PaidAmount;
    }
}