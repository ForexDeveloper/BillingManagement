using Domain.Core.Enums;

namespace Application.Query.ViewModels.TenantPlatfromContracts;

public class TenantPlatformContractFacilitatorVm
{
    public int FacilitatorId { get; set; }
    public string FacilitatorName { get; set; }
    public decimal? FixedAmountCommissionPercentage { get; set; }
    public decimal? TransactionsCommissionPercentage { get; set; }
    public BmPaymentMethodType? PaymentMethodType { get; set; }
    public string PaymentMethodTypeTitle { get; set; }
}