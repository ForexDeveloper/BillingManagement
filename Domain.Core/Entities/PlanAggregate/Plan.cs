using Domain.Base;
using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Domain.Core.Entities.PlanAggregate.Exceptions;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Core.Entities.PlanAggregate;

public class Plan : BaseEntity<int>
{
    private Plan()
    {

    }

    public Plan(int walletConfigurationId,
        string title, decimal? maxDailyWithdrawal,
        decimal maxWallet, decimal maxTotalCredit, TimeInterval billingPeriodType, int billingPeriod
        , DateTime? billingPeriodStartDate, int? gracePeriod,
        TimeInterval? paymentType, TimeInterval? installmentBreakType, int? installmentBreak,
         InstallmentPaymentMethodType? installmentPaymentMethod,
        decimal? maxDailyDeposit, decimal? maxDailyTransactionCount,
        string backgroundColor1, string backgroundColor2, string textColor,
        string description, string link, string termsAndConditions)
    {
        WalletConfigurationId = walletConfigurationId;
        Title = title;
        MaxDailyWithdrawal = maxDailyWithdrawal;
        MaxWallet = maxWallet;
        MaxTotalCredit = maxTotalCredit;
        BillingPeriodType = billingPeriodType;
        BillingPeriodStartDate = billingPeriodStartDate;
        GracePeriod = gracePeriod;
        PaymentType = paymentType;
        InstallmentBreakType = installmentBreakType;
        InstallmentPaymentMethod = installmentPaymentMethod;
        MaxDailyDeposit = maxDailyDeposit;
        MaxDailyTransactionCount = maxDailyTransactionCount;
        BackgroundColor1 = backgroundColor1;
        BackgroundColor2 = backgroundColor2;
        TextColor = textColor;
        Description = description;
        Link = link;
        InstallmentBreak = installmentBreak;
        BillingPeriod = billingPeriod;
        TermsAndConditions = termsAndConditions;
    }

    public Plan(int walletConfigurationId,
        string title, decimal? maxDailyWithdrawal,
        decimal maxWallet, decimal maxTotalCredit,
        decimal? maxDailyDeposit)
    {
        WalletConfigurationId = walletConfigurationId;
        Title = title;
        MaxDailyWithdrawal = maxDailyWithdrawal;
        MaxWallet = maxWallet;
        MaxTotalCredit = maxTotalCredit;
        MaxDailyDeposit = maxDailyDeposit;
    }

    public void SetPlan(
        string title, decimal? maxDailyWithdrawal,
        decimal maxWallet, decimal maxTotalCredit, TimeInterval billingPeriodType, int billingPeriod,
         DateTime? billingPeriodStartDate, int? gracePeriod,
        TimeInterval? paymentType, TimeInterval? installmentBreakType, int? installmentBreak,
         InstallmentPaymentMethodType? installmentPaymentMethod,
        decimal? maxDailyDeposit, decimal? maxDailyTransactionCount,
        string backgroundColor1, string backgroundColor2, string textColor,
        string description, string link, string termsAndConditions)
    {
        Title = title;
        MaxDailyWithdrawal = maxDailyWithdrawal;
        MaxWallet = maxWallet;
        MaxTotalCredit = maxTotalCredit;
        BillingPeriodType = billingPeriodType;
        BillingPeriodStartDate = billingPeriodStartDate;
        GracePeriod = gracePeriod;
        PaymentType = paymentType;
        InstallmentBreakType = installmentBreakType;
        InstallmentPaymentMethod = installmentPaymentMethod;
        MaxDailyDeposit = maxDailyDeposit;
        MaxDailyTransactionCount = maxDailyTransactionCount;
        BackgroundColor1 = backgroundColor1;
        BackgroundColor2 = backgroundColor2;
        TextColor = textColor;
        Description = description;
        Link = link;
        InstallmentBreak = installmentBreak;
        BillingPeriod = billingPeriod;
        TermsAndConditions = termsAndConditions;
    }

    public void SetPlan(
       string title, decimal? maxDailyWithdrawal,
       decimal maxWallet, decimal maxTotalCredit
     , string termsAndConditions)
    {
        Title = title;
        MaxDailyWithdrawal = maxDailyWithdrawal;
        MaxWallet = maxWallet;
        MaxTotalCredit = maxTotalCredit;
        TermsAndConditions = termsAndConditions;
    }
    public void SetPlan(decimal maxWallet, decimal? maxDailyWithdrawal, decimal? maxDailyDeposit, decimal? maxDailyTransactionCount, string termsAndConditions)
    {
        MaxWallet = maxWallet;
        MaxDailyWithdrawal = maxDailyWithdrawal;
        MaxDailyDeposit = maxDailyDeposit;
        MaxDailyTransactionCount = maxDailyTransactionCount;
        TermsAndConditions = termsAndConditions;
    }
    public void SetPlanClosedloops(PlanClosedloop planClosedloop)
    {
        PlanClosedloops ??= new List<PlanClosedloop>();
        PlanClosedloops.Add(planClosedloop);
    }

    public void SetPlanDetails(PlanDetail planDetail)
    {
        PlanDetails ??= new List<PlanDetail>();
        PlanDetails.Add(planDetail);
    }

    public void SetWalletContractPlan(WalletContractPlan walletContractPlan)
    {
        WalletContractPlans ??= new List<WalletContractPlan>();
        WalletContractPlans.Add(walletContractPlan);
    }

    public WalletConfiguration WalletConfiguration { get; private set; }
    public int WalletConfigurationId { get; private set; }
    public string Title { get; private set; }
    public decimal? MaxDailyWithdrawal { get; private set; }
    public List<PlanDetail> PlanDetails { get; private set; }
    public decimal MaxWallet { get; private set; }
    public decimal MaxTotalCredit { get; private set; }
    public decimal ReservedCredit { get; private set; }
    public decimal AssignedCredit { get; private set; }
    public TimeInterval? BillingPeriodType { get; private set; }
    public int? BillingPeriod { get; private set; }
    public DateTime? BillingPeriodStartDate { get; private set; }
    public int? GracePeriod { get; private set; }
    public TimeInterval? PaymentType { get; private set; }
    public TimeInterval? InstallmentBreakType { get; private set; }
    public int? InstallmentBreak { get; private set; }
    public InstallmentPaymentMethodType? InstallmentPaymentMethod { get; private set; }
    public bool IsActive { get; private set; } = true;
    public decimal? MaxDailyDeposit { get; private set; }
    public decimal? MaxDailyTransactionCount { get; private set; }
    public string BackgroundColor1 { get; private set; }
    public string BackgroundColor2 { get; private set; }
    public string TextColor { get; private set; }
    public string Description { get; private set; }
    public string Link { get; private set; }
    public List<PlanClosedloop> PlanClosedloops { get; private set; }
    public List<WalletContractPlan> WalletContractPlans { get; private set; }
    public string TermsAndConditions { get; set; }
    public byte[] RowVersion { get; private set; }


    public void SetTitle(string title)
    {
        if (string.IsNullOrEmpty(title?.Trim()))
        {
            throw new ArgumentException($"نام طرح وارد نشده است.");
        }
        else
        {
            Title = title;
        }
    }
    public void IncreaseReservedCreditAmount(decimal requestReserveAmount)
    {
        if (ExceedsMaxTotalCredit(requestReserveAmount))
        {
            throw new PlanMaxTotalCreditExceededException("مبلغ درخواستی بیش از سقف مبلغ تجمیعی طرح است.");
        }
        else
        {
            ReservedCredit += requestReserveAmount;
        }
    }

    public void IncreaseAssignedCreditAmount(decimal requestAssignAmount)
    {
        AssignedCredit += requestAssignAmount;
    }

    public bool ExceedsMaxTotalCredit(decimal requestAssignAmount)
    {
        return (MaxTotalCredit - (ReservedCredit + requestAssignAmount)) < 0;
    }

    public void DecreaseReservedCreditAmount(decimal requestReserveAmount)
    {
        var newReservedCredit = ReservedCredit - requestReserveAmount;

        if (newReservedCredit < 0)
        {
            throw new PlanReservedCreditNegativeException("امکان منفی شدن مبلغ اعتبار رزرو شده وجود ندارد.");
        }
        else
        {
            ReservedCredit = newReservedCredit;
        }
    }
}