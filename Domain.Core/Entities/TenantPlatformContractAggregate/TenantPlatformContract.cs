using Domain.Base;
using Domain.Core.Entities.Shared;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Core.Entities.TenantPlatformContractAggregate
{
    public class TenantPlatformContract : BaseEntity<int>
    {
        public int TenantId { get; private set; }
        public Tenant Tenant { get; private set; }
        public string ContractNumber { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public string Description { get; private set; }
        public FeeCalculationType FeeCalculationType { get; private set; }
        public BmCommissionCalculationType CommissionCalculationType { get; private set; }
        public decimal? FixedAmount { get; private set; }
        public List<TieredCommission> TieredCommissions { get; private set; }
        public decimal? FixedAmountCommission { get; private set; }
        public decimal? FixedPercentageCommission { get; private set; }
        public List<BmCommissionReferenceType> CommissionReferenceTypes { get; private set; }
        public decimal? TransactionMinCommissionAmount { get; private set; }
        public decimal? TransactionMaxCommissionAmount { get; private set; }
        public decimal? PeriodMinCommissionAmount { get; private set; }
        public decimal? PeriodMaxCommissionAmount { get; private set; }
        public TimeInterval BillingPeriodType { get; private set; }
        public int BillingPeriod { get; private set; }
        public DateTime? DailyBillingOriginDate { get; private set; }
        public int? GracePeriod { get; private set; }
        public decimal? PenaltyPercent { get; private set; }
        public List<TenantPlatformContractFacilitator> Facilitators { get; private set; } = [];
        public List<TenantPlatformContractProvider> Providers { get; private set; } = [];
        public int TenantIpgSettingId { get; private set; }
        public TenantIpgSetting TenantIpgSettings { get; private set; }
        public bool Status { get; private set; }
        public int? ParentId { get; private set; }

        private TenantPlatformContract()
        {
        }

        public TenantPlatformContract(int tenantId, string contractNumber, DateTime startDate, DateTime endDate,
           string description, FeeCalculationType feeCalculationType, BmCommissionCalculationType commissionCalculationType,
           decimal? fixedAmount, decimal? fixedAmountCommission, decimal? fixedPercentageCommission,
           List<BmCommissionReferenceType> commissionReferenceTypes,
           decimal? transactionMinCommissionAmount, decimal? transactionMaxCommissionAmount,
           decimal? periodMinCommissionAmount, decimal? periodMaxCommissionAmount,
           TimeInterval billingPeriodType, int billingPeriod, DateTime? dailyBillingOriginDate, int? gracePeriod, decimal? penaltyPercent,
           int tenantIpgSettingId)
        {
            TenantId = tenantId;
            SetContractNumber(contractNumber);
            StartDate = startDate;
            EndDate = endDate;
            Description = description;
            FeeCalculationType = feeCalculationType;
            CommissionCalculationType = commissionCalculationType;
            FixedAmount = fixedAmount;
            FixedAmountCommission = fixedAmountCommission;
            FixedPercentageCommission = fixedPercentageCommission;
            CommissionReferenceTypes = commissionReferenceTypes;
            TransactionMinCommissionAmount = transactionMinCommissionAmount;
            TransactionMaxCommissionAmount = transactionMaxCommissionAmount;
            PeriodMinCommissionAmount = periodMinCommissionAmount;
            PeriodMaxCommissionAmount = periodMaxCommissionAmount;
            BillingPeriodType = billingPeriodType;
            BillingPeriod = billingPeriod;
            DailyBillingOriginDate = dailyBillingOriginDate;
            GracePeriod = gracePeriod;
            PenaltyPercent = penaltyPercent;
            TenantIpgSettingId = tenantIpgSettingId;
            Status = true;
        }

        public void Update(int tenantId, string contractNumber, DateTime startDate, DateTime endDate,
           string description, FeeCalculationType feeCalculationType, BmCommissionCalculationType commissionCalculationType,
           decimal? fixedAmount, decimal? fixedAmountCommission, decimal? fixedPercentageCommission,
           List<BmCommissionReferenceType> commissionReferenceTypes,
           decimal? transactionMinCommissionAmount, decimal? transactionMaxCommissionAmount,
           decimal? periodMinCommissionAmount, decimal? periodMaxCommissionAmount,
           TimeInterval billingPeriodType, int billingPeriod, DateTime? dailyBillingOriginDate, int? gracePeriod, decimal? penaltyPercent,
           int tenantIpgSettingId)
        {
            TenantId = tenantId;
            SetContractNumber(contractNumber);
            StartDate = startDate;
            EndDate = endDate;
            Description = description;
            FeeCalculationType = feeCalculationType;
            CommissionCalculationType = commissionCalculationType;
            FixedAmount = fixedAmount;
            FixedAmountCommission = fixedAmountCommission;
            FixedPercentageCommission = fixedPercentageCommission;
            CommissionReferenceTypes = commissionReferenceTypes;
            TransactionMinCommissionAmount = transactionMinCommissionAmount;
            TransactionMaxCommissionAmount = transactionMaxCommissionAmount;
            PeriodMinCommissionAmount = periodMinCommissionAmount;
            PeriodMaxCommissionAmount = periodMaxCommissionAmount;
            BillingPeriodType = billingPeriodType;
            BillingPeriod = billingPeriod;
            DailyBillingOriginDate = dailyBillingOriginDate;
            GracePeriod = gracePeriod;
            PenaltyPercent = penaltyPercent;
            TenantIpgSettingId = tenantIpgSettingId;
        }

        public void SetProviders(List<TenantPlatformContractProvider> providers)
        {
            if (providers != null)
                Providers.AddRange(providers);
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
                    (CommissionCalculationType == BmCommissionCalculationType.UniformTiered ||
                    CommissionCalculationType == BmCommissionCalculationType.CumulativeTiered))
                {
                    TieredCommissions = tieredCommissions;
                }
            }
        }

        public void SetFacilitators(List<TenantPlatformContractFacilitator> facilitators)
        {
            if (facilitators != null)
                Facilitators.AddRange(facilitators);
        }

        public void SetContractNumber(string contractNumber)
        {
            if (string.IsNullOrEmpty(contractNumber))
                throw new ArgumentValidationException(nameof(ContractNumber), $"{nameof(contractNumber)} is required");

            ContractNumber = contractNumber;
        }

        public void SetStatus(bool status)
        {
            Status = status;
        }

        public void SetParentId(int parentId)
        {
            ParentId = parentId;
        }
    }
}
