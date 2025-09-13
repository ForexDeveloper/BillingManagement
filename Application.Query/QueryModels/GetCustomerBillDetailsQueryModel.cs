using Application.Query.ViewModels.Customers;
using Domain.Core.Enums;
using Domain.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Query.QueryModels;

public class GetCustomerBillDetailsQueryModel
{
    public long Id { get; set; }
    public int InstallmentNumber { get; set; }
    public int InstallmentsCount { get; set; }
    public DateTime PaymentDeadline { get; set; }
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

    public int PlanId { get; set; }
    public string WalletLogoId { get; set; }
    public BillingState BillStatus { get; set; }
    public BillingState BillStatusTitle { get; set; }
    public bool IsPayable { get; set; }
    public long InstallmentId { get; set; }
    public TimeInterval? BillingPeriodType { get; set; }
    public int? BillingPeriod { get; set; }
    public string BillingPeriodText { get; set; }

    public GetCustomerBillDetailsVm ToViewModel()
    {
        return new GetCustomerBillDetailsVm
        {
            Id = (int)this.Id,
            InstallmentNumber = this.InstallmentNumber,
            InstallmentsCount = this.InstallmentsCount,
            PaymentDeadline = this.PaymentDeadline,
            OverDueAmount = this.OverDueAmount,
            PenaltyAmount = this.PenaltyAmount,
            InstallmentAmount = this.InstallmentAmount,
            PaidAmount = this.PaidAmount,
            RemainingAmount = this.RemainingAmount,
            PayableAmount = this.PayableAmount,
            TotalAmount = this.TotalAmount,
            BillStatus = this.BillStatus,
            BillStatusTitle = this.BillStatus.GetEnumDescription(),
            IsPayable = this.IsPayable,
            WalletId = this.WalletId,
            WalletLogoId = this.WalletLogoId,
            WalletName = this.WalletName,
            BillingPeriodType = this.BillingPeriodType,
            BillingPeriod = this.BillingPeriod,
            BillingPeriodText = this.BillingPeriodText,
            PaidDetails = this.PaidDetails?
                .Select(pd => new ViewModels.Customers.PaidDetails
                {
                    Id = pd.Id,
                    Amount = pd.Amount,
                    Description = pd.Description,
                    PaymentDateTime = pd.PaymentDateTime
                })
                .OrderByDescending(pd => pd.PaymentDateTime)
                .ToList()
        };
    }
}

public class PaidDetails
{
    public long Id { get; set; }
    public string Description { get; set; }
    public DateTime PaymentDateTime { get; set; }
    public decimal Amount { get; set; }
}