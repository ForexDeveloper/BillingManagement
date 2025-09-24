using Application.Service.Dtos.Shared;
using Application.Service.Dtos.TenantMerchantContracts;
using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.TenantMerchantContracts
{
    public class CreateTenantMerchantContractModel : CreateTenantMerchantContractBaseModel
    {
        public int TenantId { get; set; }
    }

    public class CreateTenantMerchantContractBaseModel
    {
        public int MerchantId { get; set; }
        public TenantMerchantContractDocumentDto ContractDocument { get; set; }
        public string ContractNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public SettlementType SettlementType { get; set; }
        public bool IsCommissionExchanged { get; set; }
        public int? InstallmentsCount { get; set; }
        public CommissionDeductionMethodType? CommissionDeductionMethodType { get; set; }
        public decimal? InterestPercentage { get; set; }
        public List<InterestReferenceType> InterestReferenceTypes { get; set; }
        public TimeInterval BillingPeriodType { get; set; }
        public int BillingPeriod { get; set; }
        public DateTime? DailyBillingOriginDate { get; set; }
        public int? BillingBreak { get; set; }
        public PaymentMethodType PaymentMethodType { get; set; }
        public GuaranteeType? GuaranteeType { get; set; }
        public string GuaranteeDescription { get; set; }
        public CommissionCalculationType CommissionCalculationType { get; set; }
        public List<TieredCommissionDto> TieredCommissions { get; set; } = [];
        public decimal? FixedAmountCommission { get; set; }
        public decimal? FixedPercentageCommission { get; set; }
        public List<CommissionReferenceType> CommissionReferenceTypes { get; set; }
        public decimal? TransactionMinCommissionAmount { get; set; }
        public decimal? TransactionMaxCommissionAmount { get; set; }
        public decimal? PeriodMinCommissionAmount { get; set; }
        public decimal? PeriodMaxCommissionAmount { get; set; }
    }
}