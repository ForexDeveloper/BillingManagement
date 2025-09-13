using Domain.Core.Enums;
using Domain.Core.Helper;
using System;
using System.Collections.Generic;

namespace Application.Query.ViewModels.WalletConfigurations
{
    public class WalletConfigurationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public WalletType WalletTypeId { get; set; }
        public string WalletTypeTitle => WalletTypeId.GetEnumDescription();
        public decimal MaxWallet { get; set; }
        public decimal? MaximumTotalCredit { get; set; }
        public int TenantId { get; set; }
        public string TenantTitle { get; set; }
        public int? ProjectManagerId { get; set; }
        public string ProjectManagerFullName { get; set; }
        public int? CurrencyTypeId { get; set; }
        public string CurrencyTypeTitle { get; set; }
        public InstallmentsViewModel Installments { get; set; }
        public FinancialCommitmentViewModel Financial { get; set; }
        public bool Editable { get; set; }
    }

    public class InstallmentsViewModel
    {
        public List<int> MaxInstallments { get; set; }
        public decimal? PrepaymentMaxPercent { get; set; }
        public decimal? PrepaymentMinPercent { get; set; }
        public decimal? PrepaymentMaxAmount { get; set; }
        public decimal? PrepaymentMinAmount { get; set; }

    }
    public class FinancialCommitmentViewModel
    {
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
    }
}