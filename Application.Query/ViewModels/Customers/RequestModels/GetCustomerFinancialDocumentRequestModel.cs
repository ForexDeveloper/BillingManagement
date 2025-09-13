using System;
using System.Collections.Generic;
using Application.Query.Base;
using Domain.Core.Enums;

namespace Application.Query.ViewModels.Customers.RequestModels
{
    public class GetCustomerFinancialDocumentListModel : BasePaginatedListRequest
    {
        public List<FinancialDocumentType> Types { get; set; } = [];
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
