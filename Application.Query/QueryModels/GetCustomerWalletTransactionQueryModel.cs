using Application.Query.Base;
using Domain.Core.Enums;
using Domain.Core.Helper;
using System;

namespace Application.Query.QueryModels;

public class GetCustomerWalletTransactionsQueryModel : BasePaginatedListQueryResult<WalletTransactionsQueryModel>
{

}

public class WalletTransactionsQueryModel
{
    public long Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionTime { get; set; }
    public string Description { get; set; }
    public TransactionType Type { get; set; }
    public string TypeTitle => Type.GetEnumDescription();
}