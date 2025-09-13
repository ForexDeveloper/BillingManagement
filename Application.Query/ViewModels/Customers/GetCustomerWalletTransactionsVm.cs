using Application.Query.Base;
using Domain.Core.Enums;
using Domain.Core.Helper;
using System;

namespace Application.Query.ViewModels.Customers;

public class GetCustomerWalletTransactionsVm : BasePaginatedListQueryResult<CustomerWalletTransactionsVm>
{

}
public class CustomerWalletTransactionsVm
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string TypeTitle => Type.GetEnumDescription();
    public DateTime TransactionTime { get; set; }
    public string Description { get; set; }
}