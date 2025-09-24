using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels;
using Application.Query.ViewModels.Organizations;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries.Organizations;

public class GetOrganizationsQuery : BasePaginatedListRequest, IRequest<GetOrganizationsVm>
{
    public int? TenantId { get; set; }
    public GetOrganizationsQuery(int? tenantId, int pageIndex, int pageSize, string sortColumn, SortDirection? sortDirection, string searchValue)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        SortColumn = sortColumn;
        SortDirection = sortDirection;
        SearchValue = searchValue;
        TenantId = tenantId;
    }

    public class GetOrganizationsQueryHandler : BaseQueryHandler, IRequestHandler<GetOrganizationsQuery, GetOrganizationsVm>
    {
        private readonly IOrganizationReadOnlyRepository _organizationReadOnlyRepository;

        public GetOrganizationsQueryHandler(IOrganizationReadOnlyRepository organizationReadOnlyRepository)
        {
            _organizationReadOnlyRepository = organizationReadOnlyRepository;
        }

        public async Task<GetOrganizationsVm> Handle(GetOrganizationsQuery request, CancellationToken cancellationToken)
        {
            var tenantsModel = await _organizationReadOnlyRepository.GetOrganizationsAsync(request);
            return new GetOrganizationsVm()
            {
                PageIndex = tenantsModel.PageIndex,
                PageSize = tenantsModel.PageSize,
                TotalCount = tenantsModel.TotalCount,
                Items = tenantsModel.Items.Select(x =>
                new GetOrganizationVm()
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    Title = x.Title,
                    ParentId = x.ParentId,
                    ParentName = x.ParentName,
                    TenantName = x.TenantName,
                }
                ).ToList()
            };
        }
    }
}
