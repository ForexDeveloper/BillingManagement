using Application.Query.Base;
using System;

namespace Application.Query.ViewModels.Customers.Wallets
{
    public class GetCustomersWalletBillVm: BasePaginatedListQueryResult<GetCustomerWalletBillVm>
    {
    }

    public class GetCustomerWalletBillVm 
    {
        public DateTime DueDate { get; set; }
        public decimal Amount { get; set; }
    }

    public enum BillDateType
    {
        None,
        CurrentMonth,
        NextMonths,
        Overdued
    }
}
