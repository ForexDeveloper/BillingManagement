using System;
using Domain.Core.Enums;
using System.Collections.Generic;

namespace Application.Query.ViewModels.Billings;

public abstract record GetBillingViewModel
{
    public long Id { get; set; }

    public string Code { get; set; }

    public string Title { get; set; }

    public BillingStatus Status { get; set; }

    public string StatusTitle { get; set; }

    public TimeInterval PeriodType { get; set; }

    public string PeriodTypeTitle { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal PayableAmount { get; set; }

    public decimal PreviousDebitAmount { get; set; }

    public decimal PreviousCreditAmount { get; set; }

    public decimal PreviousPenaltyAmount { get; set; }

    public decimal TotalDebitAmount { get; set; }

    public decimal TotalCreditAmount { get; set; }

    public IEnumerable<GetBillingPaymentViewModel> Payments { get; set; }
}