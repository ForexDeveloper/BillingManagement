using Application.Service.Dtos.FileManagers;
using Application.Service.Dtos.Shared;
using Domain.Core.Enums;
using System;
using System.Collections.Generic;

namespace Application.Query.ViewModels.TenantMerchantContracts
{
    public class GetTenantMerchantContractVm
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string TenantName { get; set; }
        public int MerchantId { get; set; }
        public string MerchantName { get; set; }
        public IdentityTypeEnum MerchantType { get; set; }
        public string MerchantTypeTitle { get; set; }
        public string ContractNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public SettlementType SettlementType { get; set; }
        public string SettlementTypeTitle { get; set; }
        public bool IsCommissionExchanged { get; set; }
        public int? InstallmentsCount { get; set; }
        public CommissionDeductionMethodType? CommissionDeductionMethodType { get; set; }
        public string CommissionDeductionMethodTypeTitle { get; set; }
        public decimal? InterestPercentage { get; set; }
        public List<InterestReferenceType> InterestReferenceTypes { get; set; }
        public List<string> InterestReferenceTypeTitles { get; set; }
        public TimeInterval BillingPeriodType { get; set; }
        public string BillingPeriodTypeTitle { get; set; }
        public int BillingPeriod { get; set; }
        public DateTime? DailyBillingOriginDate { get; set; }
        public int? BillingBreak { get; set; }
        public PaymentMethodType PaymentMethodType { get; set; }
        public string PaymentMethodTypeTitle { get; set; }
        public GuaranteeType? GuaranteeType { get; set; }
        public string GuaranteeTypeTitle { get; set; }
        public string GuaranteeDescription { get; set; }
        public CommissionCalculationType CommissionCalculationType { get; set; }
        public string CommissionCalculationTypeTitle { get; set; }
        public List<TieredCommissionDto> TieredCommissions { get; set; } = [];
        public decimal? FixedAmountCommission { get; set; }
        public decimal? FixedPercentageCommission { get; set; }
        public List<CommissionReferenceType> CommissionReferenceTypes { get; set; }
        public List<string> CommissionReferenceTypeTitles { get; set; }
        public decimal? TransactionMinCommissionAmount { get; set; }
        public decimal? TransactionMaxCommissionAmount { get; set; }
        public decimal? PeriodMinCommissionAmount { get; set; }
        public decimal? PeriodMaxCommissionAmount { get; set; }
        public bool IsEditable { get; set; }
        public bool Status { get; set; }
        public BusinessDocumentType? BusinessDocumentType { get; set; }
        public string BusinessDocumentTypeTitle { get; set; }
        public string EnamadLink { get; set; }
        public string InternetBusinessLicenseLink { get; set; }
        public DownloadDocumentsVm NationalCartImageFront { get; set; }
        public DownloadDocumentsVm NationalCartImageBack { get; set; }
        public DownloadDocumentsVm BusinessDocumentImage { get; set; }
        public DownloadDocumentsVm OfficialNewspaper { get; set; }
    }

}
