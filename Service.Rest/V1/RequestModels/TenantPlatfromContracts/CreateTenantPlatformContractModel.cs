using Application.Service.Dtos.Shared;
using Application.Service.Dtos.TenantPlatformContract;
using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.TenantPlatfromContracts
{
    public class CreateTenantPlatformContractModel
    {
        #region BaseInfo
        public int TenantId { get; set; }
        public string ContractNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        #endregion BaseInfo

        #region FinancialInfo
        public FeeCalculationType FeeCalculationType { get; set; }
        public CommissionCalculationType CommissionCalculationType { get; set; }
        public decimal? FixedAmount { get; set; }
        public List<TieredCommissionDto> TieredCommissions { get; set; }
        public decimal? FixedAmountCommission { get; set; }
        public decimal? FixedPercentageCommission { get; set; }
        public List<CommissionReferenceType> CommissionReferenceTypes { get; set; }
        public decimal? TransactionMinCommissionAmount { get; set; }
        public decimal? TransactionMaxCommissionAmount { get; set; }
        public decimal? PeriodMinCommissionAmount { get; set; }
        public decimal? PeriodMaxCommissionAmount { get; set; }

        #endregion FinancialInfo

        #region BillSettings
        public TimeInterval BillingPeriodType { get; set; }
        public int BillingPeriod { get; set; }
        public DateTime? DailyBillingOriginDate { get; set; }
        public int? GracePeriod { get; set; }
        public decimal? PenaltyPercent { get; set; }
        #endregion BillSettings

        #region Facilitator
        public List<TenantPlatformContractFacilitatorDto> Facilitators { get; set; }
        #endregion Facilitator

        #region IpgSetting
        public int TenantIpgSettingId { get; set; }
        #endregion IpgSetting

        #region Providers
        public List<TenantPlatformContractProviderDto> Providers { get; set; }
        #endregion Providers
    }
}
