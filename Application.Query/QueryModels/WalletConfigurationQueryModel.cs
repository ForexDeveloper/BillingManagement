using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Domain.Core.Enums;
using System.Collections.Generic;

namespace Application.Query.ViewModels
{
    public class WalletConfigurationQueryModel
    {
        public WalletConfigurationQueryModel() { }
        public WalletConfigurationQueryModel(WalletConfiguration walletConfiguration)
        {
            Title = walletConfiguration.Title;
            WalletTypeId = walletConfiguration.WalletTypeId;
            MaxWallet = walletConfiguration.MaxWallet;
            MaximumTotalCredit = walletConfiguration.MaxTotalCredit;
            TenantId = walletConfiguration.TenantId;
            ProjectManagerId = walletConfiguration.ProjectManagerId;
            CurrencyTypeId = walletConfiguration.CurrencyTypeId;
            CurrencyTypeTitle = walletConfiguration.CurrencyType?.Title;
            ProjectManagerFullName = walletConfiguration.ProjectManager?.FullName;
            TenantTitle = walletConfiguration.Tenant.Title;
            Id = walletConfiguration.Id;
            MaxInstallments = walletConfiguration.MaxInstallments;
            PrepaymentMinPercent = walletConfiguration.PrepaymentMinPercent;
            PrepaymentMaxAmount = walletConfiguration.PrepaymentMaxAmount;
            PrepaymentMaxPercent = walletConfiguration.PrepaymentMaxPercent;
            PrepaymentMinAmount = walletConfiguration.PrepaymentMinAmount;
            InterestPeriodMaxPercent = walletConfiguration.InterestPeriodMaxPercent;
            InterestPeriodMinPercent = walletConfiguration.InterestPeriodMinPercent;
            InterestPeriodMaxAmount = walletConfiguration.InterestPeriodMaxAmount;
            InterestPeriodMinAmount = walletConfiguration.InterestPeriodMinAmount;
            PenaltyPeriodMaxPercent = walletConfiguration.PenaltyPeriodMaxPercent;
            PenaltyPeriodMinPercent = walletConfiguration.PenaltyPeriodMinPercent;
            PenaltyPeriodMaxAmount = walletConfiguration.PenaltyPeriodMaxAmount;
            PenaltyPeriodMinAmount = walletConfiguration.PenaltyPeriodMinAmount;
            WaiverPeriodMinPercent = walletConfiguration.WaiverPeriodMinPercent;
            WaiverPeriodMaxPercent = walletConfiguration.WaiverPeriodMaxPercent;
            WaiverPeriodMinAmount = walletConfiguration.WaiverPeriodMinAmount;
            WaiverPeriodMaxAmount = walletConfiguration.WaiverPeriodMaxAmount;
        }
        #region Wallet
        public int Id { get; set; }
        public string Title { get; set; }
        public WalletType WalletTypeId { get; set; }
        public decimal MaxWallet { get; set; }
        public decimal? MaximumTotalCredit { get; set; }
        public int TenantId { get; set; }
        public string TenantTitle { get; set; }
        public int? ProjectManagerId { get; set; }
        public int? CurrencyTypeId { get; set; }
        public string ProjectManagerFullName { get; set; }
        public string CurrencyTypeTitle { get; set; }

        #endregion #region Property

        #region Installments
        public List<int> MaxInstallments { get; set; }
        public decimal? PrepaymentMaxPercent { get; set; }
        public decimal? PrepaymentMinPercent { get; set; }
        public decimal? PrepaymentMaxAmount { get; set; }
        public decimal? PrepaymentMinAmount { get; set; }

        #endregion #region Property

        #region Financial
        public decimal? InterestPeriodMaxPercent { get; set; }
        public decimal? InterestPeriodMinPercent { get; set; }
        public decimal? InterestPeriodMaxAmount { get; set; }
        public decimal? InterestPeriodMinAmount { get; set; }
        public decimal? PenaltyPeriodMaxPercent { get; set; }
        public decimal? PenaltyPeriodMinPercent { get; set; }
        public decimal? PenaltyPeriodMaxAmount { get; set; }
        public decimal? PenaltyPeriodMinAmount { get; set; }
        public decimal? WaiverPeriodMaxPercent { get; set; }
        public decimal? WaiverPeriodMinPercent { get; set; }
        public decimal? WaiverPeriodMaxAmount { get; set; }
        public decimal? WaiverPeriodMinAmount { get; set; }

        #endregion #region Property

    }
}