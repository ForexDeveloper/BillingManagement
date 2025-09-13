using Application.Query.ViewModels.Customers.Wallets;
using Domain.Core.Enums;
using System;
using System.Collections.Generic;

namespace Application.Query.ViewModels.Customers;

public class GetCustomerBillDetailsVm
{
    public int Id { get; set; }
    public int InstallmentNumber { get; set; }
    public int InstallmentsCount { get; set; }
    public DateTime PaymentDeadline { get; set; }
    public BillDateType DateType { get; set; }
    public decimal OverDueAmount { get; set; }
    public decimal PenaltyAmount { get; set; }
    public decimal InstallmentAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal PayableAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public IEnumerable<PaidDetails> PaidDetails { get; set; }
    public int WalletId { get; set; }
    public string WalletName { get; set; }
    public string WalletLogoId { get; set; }
    public BillingState BillStatus { get; set; }
    public string BillStatusTitle { get; set; }
    public bool IsPayable { get; set; }
    public TimeInterval? BillingPeriodType { get; set; }
    public int? BillingPeriod { get; set; }
    public string BillingPeriodText { get; set; }
}

public class PaidDetails
{
    public long Id { get; set; }
    public string Description { get; set; }
    public DateTime PaymentDateTime { get; set; }
    public decimal Amount { get; set; }

}