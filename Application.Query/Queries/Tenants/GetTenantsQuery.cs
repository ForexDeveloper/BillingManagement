using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels;
using Application.Query.ViewModels.Tenants;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries.Tenants;

public class GetTenantsQuery : BasePaginatedListRequest, IRequest<GetTenantsVm>
{
    public GetTenantsQuery(int pageIndex, int pageSize, string sortColumn, SortDirection? sortDirection, string searchValue)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        SortColumn = sortColumn;
        SortDirection = sortDirection;
        SearchValue = searchValue;
    }

    public class GetTenantsQueryHandler : BaseQueryHandler, IRequestHandler<GetTenantsQuery, GetTenantsVm>
    {
        private readonly ITenantReadOnlyRepository _tenantReadOnlyRepository;

        public GetTenantsQueryHandler(ITenantReadOnlyRepository tenantReadOnlyRepository)
        {
            _tenantReadOnlyRepository = tenantReadOnlyRepository;
        }

        public async Task<GetTenantsVm> Handle(GetTenantsQuery request, CancellationToken cancellationToken)
        {
            var tenantsModel = await _tenantReadOnlyRepository.GetTenantsAsync(request);

            return new GetTenantsVm()
            {
                PageIndex = tenantsModel.PageIndex,
                PageSize = tenantsModel.PageSize,
                TotalCount = tenantsModel.TotalCount,
                Items = tenantsModel.Items.Select(x =>
                new TenantVM()
                {
                    Id = x.Id,
                    Title = x.Title,
                }).ToList()
            };
        }
    }
}
