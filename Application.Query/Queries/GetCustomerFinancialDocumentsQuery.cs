using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Customers;
using Domain.Core.Enums;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Application.Query.Queries
{
    public class GetCustomerFinancialDocumentsQuery : IRequest<GetCustomerFinancialDocumentsVm>
    {
        public GetCustomerFinancialDocumentsQuery(int id, DateTime? fromDate, DateTime? toDate, List<FinancialDocumentType> types,
            int pageIndex, int pageSize, string searchValue, string sortColumn,
            SortDirection? sortDirection, int? tenantId = null)
        {
            Id = id;
            FromDate = fromDate;
            ToDate = toDate;
            Types = types;
            PageIndex = pageIndex;
            PageSize = pageSize;
            SearchValue = searchValue;
            SortColumn = sortColumn;
            SortDirection = sortDirection;
            TenantId = tenantId;
        }

        public int Id { get; set; }
        public int? TenantId { get; set; }
        public DateTime? FromDate { get; }
        public DateTime? ToDate { get; }
        public List<FinancialDocumentType> Types { get; set; } = [];
        public int PageIndex { get; }
        public int PageSize { get; }
        public string SearchValue { get; }
        public string SortColumn { get; }
        public SortDirection? SortDirection { get; }
    }


    public class GetCustomerFinancialDocumentsQueryHandler : IRequestHandler<GetCustomerFinancialDocumentsQuery, GetCustomerFinancialDocumentsVm>
    {
        private readonly IFinancialDocumentReadOnlyRepository _financialDocumentReadOnlyRepository;

        public GetCustomerFinancialDocumentsQueryHandler(IFinancialDocumentReadOnlyRepository financialDocumentReadOnlyRepository)
        {
            _financialDocumentReadOnlyRepository = financialDocumentReadOnlyRepository;
        }

        public async Task<GetCustomerFinancialDocumentsVm> Handle(GetCustomerFinancialDocumentsQuery request, CancellationToken cancellationToken)
        {
            var result = await _financialDocumentReadOnlyRepository.GetCustomerFinancialDocumentsAsync(request, cancellationToken);

            return new GetCustomerFinancialDocumentsVm
            {
                Items = result.Items.Select(p => new CustomerFinancialDocumentVm
                {
                    Amount = p.Amount,
                    Description = p.Description,
                    Time = p.TransactionTime,
                    Id = p.Id,
                    Type = p.Type
                }).ToList(),
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            };
        }
    }
}
