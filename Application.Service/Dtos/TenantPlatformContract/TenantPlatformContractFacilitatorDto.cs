using Domain.Core.Enums;

namespace Application.Service.Dtos.TenantPlatformContract;

public class TenantPlatformContractFacilitatorDto
{
    public int FacilitatorId { get; set; }
    public decimal? FixedAmountCommissionPercentage { get; set; }
    public decimal? TransactionsCommissionPercentage { get; set; }
    public PaymentMethodType? PaymentMethodType { get; set; }
}