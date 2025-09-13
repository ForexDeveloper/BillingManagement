using Application.Query.Base;
using Application.Query.ViewModels.Customers;
using Domain.Core.Enums;
using System;
using System.Linq;

namespace Application.Query.QueryModels
{
    public class CustomerFinancialDocumentsQueryModel : BasePaginatedListQueryResult<CustomerFinancialDocumentQueryModel>
    {
        internal GetCustomerFinancialDocumentsVm toViewModel()
        {
            return new GetCustomerFinancialDocumentsVm
            {
                Items = base.Items.Select(item => new CustomerFinancialDocumentVm
                {
                    Amount = item.Amount,
                    Id = item.Id,
                    Description = item.Description,
                    Time= item.TransactionTime,
                    Type = item.Type
                }).ToList(),
                PageIndex = base.PageIndex,
                PageSize = base.PageSize,
                TotalCount = base.TotalCount
            };
        }
    }

    public class CustomerFinancialDocumentQueryModel
    {
        public long Id { get; set; }
        public FinancialDocumentType Type { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionTime { get; set; }
        public string Description { get; set; }
    }
}
