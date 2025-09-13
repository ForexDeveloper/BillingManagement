using Domain.Core.Enums;
using Domain.Core.Helper;
using System;

namespace Application.Query.ViewModels.Customers
{
    public class GetCustomerWalletTransactionVm
    {
        public long Id { get; set; }
        public TransactionType Type { get; set; }
        public string TypeTitle => Type.GetEnumDescription();
        public decimal Amount { get; set; }
        public DateTime TransactionTime { get; set; }
    }
}
