using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Customers;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetCustomerListQuery : BasePaginatedListRequest, IRequest<GetCustomersVm>
    {
        public GetCustomerListQuery(BasePaginatedListRequest query, int tenantId)
        {
            PageIndex = query.PageIndex;
            PageSize = query.PageSize;
            SortColumn = query.SortColumn;
            SortDirection = query.SortDirection;
            SearchValue = query.SearchValue;
            TenantId = tenantId;
        }
        public int TenantId { get; set; }
    }
    public class CustomerListQueryHandler : BaseQueryHandler, IRequestHandler<GetCustomerListQuery, GetCustomersVm>
    {
        private readonly ICustomerReadOnlyRepository _customerReadOnlyRepository;
        public CustomerListQueryHandler(ICustomerReadOnlyRepository customerReadOnlyRepository)
        {
            _customerReadOnlyRepository = customerReadOnlyRepository;
        }

        public async Task<GetCustomersVm> Handle(GetCustomerListQuery request, CancellationToken cancellationToken)
        {
            var result = await _customerReadOnlyRepository.GetCustomerListAsync(request);

            return new GetCustomersVm
            {
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                Items = result.Items.Select(c => new GetCustomerVm
                {
                    Id = c.Id,
                    TenantId = c.TenantId,
                    TenantName = c.TenantName,
                    FullName = c.FullName,
                    Mobile = c.Mobile,
                    NationalId = c.NationalId
                }).ToList()
            };
        }
    }
}
