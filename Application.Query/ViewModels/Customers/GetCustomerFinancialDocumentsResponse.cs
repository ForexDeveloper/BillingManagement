using Application.Query.Base;
using Domain.Core.Enums;
using Domain.Core.Helper;
using System;

namespace Application.Query.ViewModels.Customers
{
    public class GetCustomerFinancialDocumentsVm : BasePaginatedListQueryResult<CustomerFinancialDocumentVm>
    {

    }
    public class CustomerFinancialDocumentVm
    {
        public long Id { get; set; }
        public FinancialDocumentType Type { get; set; }
        public string TypeTitle => Type.GetEnumDescription();
        public decimal Amount { get; set; }
        public DateTime Time { get; set; }
        public string Description { get; set; }
    }
}
