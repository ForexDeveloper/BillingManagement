using Application.Query.Base;
using Application.Query.ViewModels.Customers.Wallets;
using Domain.Core.Enums;
using Shared.Utilities.Extensions;
using System;
using System.Linq;

namespace Application.Query.QueryModels;

public class BillQueryModel
{
    public long Id { get; set; }
    public DateTime DueDate { get; set; }
    public decimal PayableAmount { get; set; }
    public int WalletId { get; set; }
    public string WalletName { get; set; }
    public string WalletLogoId { get; set; }
    public TimeInterval BillingPeriodType { get; set; }
    public int BillingPeriod { get; set; }
    public BillingState BillingStatus { get; set; }
    public int PlanId { get; set; }
}
public class BillsQueryModel : BasePaginatedListQueryResult<BillQueryModel>
{
    public GetCustomerBillsVm ToViewModel()
    {
        return new GetCustomerBillsVm
        {
            PageIndex = base.PageIndex,
            PageSize = base.PageSize,
            TotalCount = base.TotalCount,
            Items = base.Items?.Select(b => new CustomerBillVm
            {
                Id = b.Id,
                DueDate = b.DueDate,
                Amount = b.PayableAmount,
                PayableAmount = b.PayableAmount,
                WalletId = b.WalletId,
                WalletName = b.WalletName,
                WalletLogoId = b.WalletLogoId,
                BillingPeriodType = b.BillingPeriodType,
                BillingPeriod = b.BillingPeriod,
                BillingPeriodText = b.BillingPeriodType != TimeInterval.Day ? b.BillingPeriodType.GetEnumDescription() : b.BillingPeriod + " روز یکبار",
                BillingStatus = b.BillingStatus
            }).ToList()
        };
    }
}
public class CustomersQueryModel : BasePaginatedListQueryResult<CustomerQueryModel>
{

}

