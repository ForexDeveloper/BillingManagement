using System;
using Domain.Core.Enums;

namespace Application.Query.ViewModels.Billings;

public abstract record GetBillingVm
{
    public long Id { get; set; }

    public string Code { get; set; }

    public string Title { get; set; }

    public BillingType Type { get; set; }

    public string TypeTitle { get; set; }

    public BillingStatus Status { get; set; }

    public string StatusTitle { get; set; }

    public bool CanSetAdditions { get; set; }

    public bool CanSetDeductions { get; set; }

    public TimeInterval PeriodType { get; set; }

    public string PeriodTypeTitle { get; set; }

    public bool IsCommissionExchanged { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime DueDate { get; set; }

    public bool IsPayable { get; set; }

    public decimal Amount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal PayableAmount { get; set; }

    public decimal Additions { get; set; }

    public decimal Deductions { get; set; }

    public decimal PreviousDebitAmount { get; set; }

    public decimal PreviousCreditAmount { get; set; }

    public decimal PreviousPenaltyAmount { get; set; }

    public decimal TotalDebitAmount { get; set; }

    public decimal TotalCreditAmount { get; set; }
}