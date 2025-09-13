using Domain.Base;
using Domain.Core.Entities.ClosedloopAggregate;
using Domain.Core.Entities.PlanAggregate;
using Domain.Core.Entities.ProjectManegerAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Core.AggregateRoots.WalletConfigurationAggregate
{
    [Serializable]
    public class WalletConfiguration : BaseEntity<int>
    {
        public string Title { get; private set; }
        public WalletType WalletTypeId { get; private set; }
        public decimal MaxWallet { get; private set; }
        public decimal? MaxTotalCredit { get; private set; }
        public int TenantId { get; private set; }
        public Tenant Tenant { get; private set; }
        public int? ProjectManagerId { get; private set; }
        public ProjectManager ProjectManager { get; private set; }
        public int? CurrencyTypeId { get; private set; }
        public CurrencyType CurrencyType { get; private set; }
        public List<Plan> Plans { get; private set; }


        #region Installments

        public List<int> MaxInstallments { get; set; }
        public decimal? PrepaymentMinPercent { get; set; }
        public decimal? PrepaymentMaxPercent { get; set; }
        public decimal? PrepaymentMaxAmount { get; set; }
        public decimal? PrepaymentMinAmount { get; set; }

        #endregion #region Property

        #region Financial
        public decimal? InterestPeriodMaxPercent { get; private set; }
        public decimal? InterestPeriodMinPercent { get; private set; }
        public decimal? InterestPeriodMaxAmount { get; private set; }
        public decimal? InterestPeriodMinAmount { get; private set; }
        public decimal? PenaltyPeriodMaxPercent { get; private set; }
        public decimal? PenaltyPeriodMinPercent { get; private set; }
        public decimal? PenaltyPeriodMaxAmount { get; private set; }
        public decimal? PenaltyPeriodMinAmount { get; private set; }
        public decimal? WaiverPeriodMaxPercent { get; private set; }
        public decimal? WaiverPeriodMinPercent { get; private set; }
        public decimal? WaiverPeriodMaxAmount { get; private set; }
        public decimal? WaiverPeriodMinAmount { get; private set; }

        #endregion #region Property
        public List<Closedloop> Closedloops { get; private set; }

        private WalletConfiguration()
        {
        }

        public WalletConfiguration(string title,
            int tenantId, WalletType walletTypeId,
            int? projectManagerId, int? currencyTypeId,
            decimal maxWallet, decimal? maximumTotalCredit)
        {
            SetWalletConfiguration(title, tenantId, walletTypeId, projectManagerId, currencyTypeId, maxWallet, maximumTotalCredit);
        }

        public void SetWalletConfiguration(string title,
            int tenantId, WalletType walletTypeId,
            int? projectManagerId, int? currencyTypeId,
            decimal maxWallet, decimal? maximumTotalCredit)
        {

            if (string.IsNullOrEmpty(title))
                throw new ArgumentValidationException(nameof(Title), $"{nameof(title)} title  is required");

            if (walletTypeId is 0 || walletTypeId > WalletType.BNPL)
                throw new ArgumentValidationException(nameof(WalletTypeId), $"{nameof(walletTypeId)} wallet Type is required");

            if (maxWallet is 0)
                throw new ArgumentValidationException(nameof(MaxWallet), $"{nameof(maxWallet)} max wallet Type is required");

            Title = title;
            TenantId = tenantId;
            WalletTypeId = walletTypeId;
            ProjectManagerId = projectManagerId;
            CurrencyTypeId = currencyTypeId;
            MaxWallet = maxWallet;
            MaxTotalCredit = maximumTotalCredit;
        }

        public void SetInstallmentsInfo(List<int> maxInstallments,
            decimal? preminpayment,
           decimal? premaxpayment,
            decimal? maxAmount,
            decimal? minAmount
            )
        {

            if (maxInstallments is null || !maxInstallments.Any())
                throw new ArgumentValidationException(nameof(MaxInstallments), $"{nameof(maxInstallments)} maxInstallments Type is required");

            MaxInstallments = maxInstallments;
            PrepaymentMinPercent = preminpayment;
            PrepaymentMaxPercent = premaxpayment;
            PrepaymentMaxAmount = maxAmount;
            PrepaymentMinAmount = minAmount;
        }

        public void UpdateInstallmentsInfo(List<int> maxInstallments,
                    decimal? preminpayment,
           decimal? premaxpayment,
            decimal? maxAmount,
            decimal? minAmount
            )
        {
            if (maxInstallments is null || !maxInstallments.Any())
                throw new ArgumentValidationException(nameof(MaxInstallments), $"{nameof(maxInstallments)} maxInstallments Type is required");

            MaxInstallments = maxInstallments;
            PrepaymentMinPercent = preminpayment;
            PrepaymentMaxPercent = premaxpayment;
            PrepaymentMaxAmount = maxAmount;
            PrepaymentMinAmount = minAmount;
        }
        public void SetMaxWallet(decimal maxWallet)
        {
            MaxWallet = maxWallet;
        }
        public void SetFinancialInfo(decimal? interestPeriodMaxPercent,
            decimal? interestPeriodMinPercent, decimal? interestPeriodMaxAmount,
            decimal? interestPeriodMinAmount,
            decimal? penaltyPeriodMaxPercent,
             decimal? penaltyPeriodMinPercent,
             decimal? penaltyPeriodMaxAmount,
             decimal? penaltyPeriodMinAmount,
             decimal? waiverPeriodMaxPercent,
             decimal? waiverPeriodMinPercent,
             decimal? waiverPeriodMaxAmount,
             decimal? waiverPeriodMinAmount
            )
        {
            InterestPeriodMaxPercent = interestPeriodMaxPercent;
            InterestPeriodMinPercent = interestPeriodMinPercent;
            InterestPeriodMaxAmount = interestPeriodMaxAmount;
            InterestPeriodMinAmount = interestPeriodMinAmount;
            PenaltyPeriodMaxPercent = penaltyPeriodMaxPercent;
            PenaltyPeriodMinPercent = penaltyPeriodMinPercent;
            PenaltyPeriodMaxAmount = penaltyPeriodMaxAmount;
            PenaltyPeriodMinAmount = penaltyPeriodMinAmount;
            WaiverPeriodMaxPercent = waiverPeriodMaxPercent;
            WaiverPeriodMinPercent = waiverPeriodMinPercent;
            WaiverPeriodMaxAmount = waiverPeriodMaxAmount;
            WaiverPeriodMinAmount = waiverPeriodMinAmount;
        }

        public void UpdateFinancialInfo(decimal? interestPeriodMaxPercent,
            decimal? interestPeriodMinPercent, decimal? interestPeriodMaxAmount,
            decimal? interestPeriodMinAmount,
            decimal? penaltyPeriodMaxPercent,
             decimal? penaltyPeriodMinPercent,
             decimal? penaltyPeriodMaxAmount,
             decimal? penaltyPeriodMinAmount,
             decimal? waiverPeriodMaxPercent,
             decimal? waiverPeriodMinPercent,
             decimal? waiverPeriodMaxAmount,
             decimal? waiverPeriodMinAmount
            )
        {
            InterestPeriodMaxPercent = interestPeriodMaxPercent;
            InterestPeriodMinPercent = interestPeriodMinPercent;
            InterestPeriodMaxAmount = interestPeriodMaxAmount;
            InterestPeriodMinAmount = interestPeriodMinAmount;
            PenaltyPeriodMaxPercent = penaltyPeriodMaxPercent;
            PenaltyPeriodMinPercent = penaltyPeriodMinPercent;
            PenaltyPeriodMaxAmount = penaltyPeriodMaxAmount;
            PenaltyPeriodMinAmount = penaltyPeriodMinAmount;
            WaiverPeriodMaxPercent = waiverPeriodMaxPercent;
            WaiverPeriodMinPercent = waiverPeriodMinPercent;
            WaiverPeriodMaxAmount = waiverPeriodMaxAmount;
            WaiverPeriodMinAmount = waiverPeriodMinAmount;
        }
        public void SetPlan(Plan plan)
        {
            Plans ??= new List<Plan>();
            Plans.Add(plan);
        }
    }

}