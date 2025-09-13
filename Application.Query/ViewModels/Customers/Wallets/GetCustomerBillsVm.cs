using Application.Query.Base;
using Domain.Core.Enums;
using System;

namespace Application.Query.ViewModels.Customers.Wallets;

public class GetCustomerBillsVm : BasePaginatedListQueryResult<CustomerBillVm>
{

}

public class CustomerBillVm
{
    public long Id { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal PayableAmount { get; set; }
    public int WalletId { get; set; }
    public string WalletName { get; set; }
    public string WalletLogoId { get; set; }
    public TimeInterval BillingPeriodType { get; set; }
    public int BillingPeriod { get; set; }
    public string BillingPeriodText { get; set; }
    public BillingState BillingStatus { get; set; }
}
