using System.Collections.Generic;
using Application.Query.Base;
using Domain.Core.Enums;

namespace Application.Query.ViewModels.Customers.RequestModels
{
    public class GetWalletTransactionsRequest : BasePaginatedListRequest
    {
        public List<TransactionType> Types { get; set; } = [];
    }
}
