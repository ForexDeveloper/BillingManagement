using Domain.Base;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.MerchantAggregate;
using Domain.Core.Entities.Shared;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Enums;
using Domain.Core.Helper;
using System;
using System.Collections.Generic;

namespace Domain.Core.Entities.TenantMerchantContractAggregate;

public class TenantMerchantContract : BaseEntity<int>
{
    public int TenantId { get; private set; }

    public Tenant Tenant { get; private set; }

    public int MerchantId { get; private set; }

    public Merchant Merchant { get; private set; }

    public string EnamadLink { get; private set; }

    public string InternetBusinessLicenseLink { get; private set; }

    public string ContractNumber { get; private set; }

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    public bool Status { get; private set; }

    public SettlementType SettlementType { get; private set; }

    public bool IsCommissionExchanged { get; private set; }

    public int InstallmentsCount { get; private set; }

    public CommissionDeductionMethodType? CommissionDeductionMethodType { get; private set; }

    public decimal? InterestPercentage { get; private set; }

    public List<InterestReferenceType> InterestReferenceTypes { get; private set; }

    public TimeInterval BillingPeriodType { get; private set; }

    public int BillingPeriod { get; private set; }

    public DateTime? DailyBillingOriginDate { get; private set; }

    public int? BillingBreak { get; private set; }

    public PaymentMethodType PaymentMethodType { get; private set; }

    public GuaranteeType? GuaranteeType { get; private set; }

    public string GuaranteeDescription { get; private set; }

    public CommissionCalculationType CommissionCalculationType { get; private set; }

    public List<TieredCommission> TieredCommissions { get; private set; }

    public decimal? FixedAmountCommission { get; private set; }

    public decimal? FixedPercentageCommission { get; private set; }

    public List<CommissionReferenceType> CommissionReferenceTypes { get; private set; }

    public decimal? TransactionMinCommissionAmount { get; private set; }

    public decimal? TransactionMaxCommissionAmount { get; private set; }

    public decimal? PeriodMinCommissionAmount { get; private set; }

    public decimal? PeriodMaxCommissionAmount { get; private set; }

    public int? ParentId { get; private set; }

    public TenantMerchantContract? Parent { get; private set; }

    public ICollection<TenantMerchantContract>? Children { get; private set; }

    public ICollection<FinancialDocument> FinancialDocuments { get; private set; }

    private TenantMerchantContract()
    {

    }

    public TenantMerchantContract(int tenantId, int merchantId,
        string contractNumber, DateTime startDate, DateTime endDate,
        SettlementType settlementType, bool isCommissionExchanged, int? installmentsCount,
        CommissionDeductionMethodType? commissionDeductionMethodType,
        decimal? interestPercentage, List<InterestReferenceType> interestReferenceTypes,
        TimeInterval billingPeriodType, int billingPeriod, DateTime? dailyBillingOriginDate, int? billingBreak,
        PaymentMethodType paymentMethodType,
        GuaranteeType? guaranteeType, string guaranteeDescription,
        CommissionCalculationType commissionCalculationType,
        decimal? fixedAmountCommission, decimal? fixedPercentageCommission,
        List<CommissionReferenceType> commissionReferenceTypes,
        decimal? transactionMinCommissionAmount, decimal? transactionMaxCommissionAmount,
        decimal? periodMinCommissionAmount, decimal? periodMaxCommissionAmount)
    {
        TenantId = tenantId;
        MerchantId = merchantId;
        ContractNumber = contractNumber;
        StartDate = startDate;
        EndDate = endDate;
        SettlementType = settlementType;
        IsCommissionExchanged = isCommissionExchanged;
        SetInstallmentsCount(installmentsCount);
        CommissionDeductionMethodType = commissionDeductionMethodType;
        InterestPercentage = interestPercentage;
        InterestReferenceTypes = interestReferenceTypes;
        BillingPeriodType = billingPeriodType;
        BillingPeriod = billingPeriod;
        DailyBillingOriginDate = dailyBillingOriginDate;
        BillingBreak = billingBreak;
        PaymentMethodType = paymentMethodType;
        GuaranteeType = guaranteeType;
        GuaranteeDescription = guaranteeDescription;
        CommissionCalculationType = commissionCalculationType;
        FixedAmountCommission = fixedAmountCommission;
        FixedPercentageCommission = fixedPercentageCommission;
        CommissionReferenceTypes = commissionReferenceTypes;
        TransactionMinCommissionAmount = transactionMinCommissionAmount;
        TransactionMaxCommissionAmount = transactionMaxCommissionAmount;
        PeriodMinCommissionAmount = periodMinCommissionAmount;
        PeriodMaxCommissionAmount = periodMaxCommissionAmount;
        Status = true;
    }

    public void Update(int tenantId, int merchantId,
        string contractNumber, DateTime startDate, DateTime endDate,
        SettlementType settlementType, bool isCommissionExchanged, int? installmentsCount,
        CommissionDeductionMethodType? commissionDeductionMethodType,
        decimal? interestPercentage, List<InterestReferenceType> interestReferenceTypes,
        TimeInterval billingPeriodType, int billingPeriod, DateTime? dailyBillingOriginDate, int? billingBreak,
        PaymentMethodType paymentMethodType,
        GuaranteeType? guaranteeType, string guaranteeDescription,
        CommissionCalculationType commissionCalculationType,
        decimal? fixedAmountCommission, decimal? fixedPercentageCommission,
        List<CommissionReferenceType> commissionReferenceTypes,
        decimal? transactionMinCommissionAmount, decimal? transactionMaxCommissionAmount,
        decimal? periodMinCommissionAmount, decimal? periodMaxCommissionAmount)
    {
        TenantId = tenantId;
        MerchantId = merchantId;
        ContractNumber = contractNumber;
        StartDate = startDate;
        EndDate = endDate;
        SettlementType = settlementType;
        IsCommissionExchanged = isCommissionExchanged;
        SetInstallmentsCount(installmentsCount);
        CommissionDeductionMethodType = commissionDeductionMethodType;
        InterestPercentage = interestPercentage;
        InterestReferenceTypes = interestReferenceTypes;
        BillingPeriodType = billingPeriodType;
        BillingPeriod = billingPeriod;
        DailyBillingOriginDate = dailyBillingOriginDate;
        BillingBreak = billingBreak;
        PaymentMethodType = paymentMethodType;
        GuaranteeType = guaranteeType;
        GuaranteeDescription = guaranteeDescription;
        CommissionCalculationType = commissionCalculationType;
        FixedAmountCommission = fixedAmountCommission;
        FixedPercentageCommission = fixedPercentageCommission;
        CommissionReferenceTypes = commissionReferenceTypes;
        TransactionMinCommissionAmount = transactionMinCommissionAmount;
        TransactionMaxCommissionAmount = transactionMaxCommissionAmount;
        PeriodMinCommissionAmount = periodMinCommissionAmount;
        PeriodMaxCommissionAmount = periodMaxCommissionAmount;
    }
    public void SetEnamadLink(string enamadLink)
    {
        if (!string.IsNullOrEmpty(enamadLink) && !BaseValidationHelpers.IsValidUrl(enamadLink))
        {
            throw new ArgumentValidationException(nameof(EnamadLink), $"{nameof(enamadLink)} is not valid");
        }

        EnamadLink = enamadLink;
    }

    public void SetInternetBusinessLicenseLink(string internetBusinessLicenseLink)
    {
        if (!string.IsNullOrEmpty(internetBusinessLicenseLink) && !BaseValidationHelpers.IsValidUrl(internetBusinessLicenseLink))
        {
            throw new ArgumentValidationException(nameof(InternetBusinessLicenseLink), $"{nameof(internetBusinessLicenseLink)} is not valid");
        }

        InternetBusinessLicenseLink = internetBusinessLicenseLink;
    }

    public void SetGuaranteeDescription(string guaranteeDescription)
    {
        GuaranteeDescription = GuaranteeType == Enums.GuaranteeType.Other ? guaranteeDescription : null;
    }

    public void SetStatus(bool status)
    {
        Status = status;
    }

    public void SetParentId(int parentId)
    {
        ParentId = parentId;
    }

    public void SetTieredCommissions(List<TieredCommission>? tieredCommissions)
    {
        if (tieredCommissions == null || tieredCommissions.Count == 0)
        {
            TieredCommissions = null;
        }
        else
        {
            TieredCommission.ValidateInputList(tieredCommissions);

            if (tieredCommissions != null &&
                (CommissionCalculationType == CommissionCalculationType.UniformTiered ||
                CommissionCalculationType == CommissionCalculationType.CumulativeTiered))
            {
                TieredCommissions = tieredCommissions;
            }
        }
    }

    public void SetContractNumber(string contractNumber)
    {
        if (string.IsNullOrEmpty(contractNumber))
            throw new ArgumentValidationException(nameof(ContractNumber), $"{nameof(contractNumber)} is required");

        ContractNumber = contractNumber;
    }

    public void SetInstallmentsCount(int? installmentsCount)
    {
        InstallmentsCount = SettlementType == SettlementType.LumpSum ? 1 : installmentsCount.Value;
    }
}