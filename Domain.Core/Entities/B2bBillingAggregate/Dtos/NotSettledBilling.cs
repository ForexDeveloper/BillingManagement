using Domain.Core.Entities.MerchantBillingAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate;

namespace Domain.Core.Entities.B2bBillingAggregate.Dtos;

public sealed record NotSettledBilling
{
    public decimal PaidAmount { get; set; }

    public int ChildContractId { get; set; }

    public MerchantBilling Billing { get; set; }

    public TenantMerchantContract Contract { get; set; }

    public decimal CalculatePayableAmount()
    {
        return Billing.Amount - PaidAmount;
    }
}