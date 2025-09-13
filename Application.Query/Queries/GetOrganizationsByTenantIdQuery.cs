using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Organizations;
using Domain.Core.Entities.OrganizationAggregate.Exceptions;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries;

public class GetOrganizationsByTenantIdQuery : IRequest<GetOrganizationsTreeVm>
{
    public int TenantId { get; }
    public GetOrganizationsByTenantIdQuery(int tenantId)
    {
        TenantId = tenantId;
    }

    public class GetOrganizationsByTenantIdQueryHandler : BaseQueryHandler, IRequestHandler<GetOrganizationsByTenantIdQuery, GetOrganizationsTreeVm>
    {
        private readonly IOrganizationReadOnlyRepository _organizationReadOnlyRepository;

        public GetOrganizationsByTenantIdQueryHandler(IOrganizationReadOnlyRepository organizationReadOnlyRepository)
        {
            _organizationReadOnlyRepository = organizationReadOnlyRepository;
        }

        public async Task<GetOrganizationsTreeVm> Handle(GetOrganizationsByTenantIdQuery request, CancellationToken cancellationToken)
        {
            var organizations = await _organizationReadOnlyRepository.GetByTenantIdAsync(request.TenantId);
            if (!organizations.Any())
                throw new OrganizationNotFoundException("سازمان پیدا نشد.");

            var organizationsVm = ToViewModel(organizations);
            if (!organizationsVm.Any())
                throw new OrganizationNotFoundException("سازمان پیدا نشد.");

            var result = new GetOrganizationsTreeVm
            {
                Organizations = organizationsVm
            };

            return result;
        }
    }

    public static List<OrganizationsVm> ToViewModel(List<OrganizationsVm> organizations)
    {
        var organizationsDictionary = new Dictionary<int, OrganizationsVm>();

        foreach (var organization in organizations)
        {
            organizationsDictionary[organization.Id] = organization;
        }

        foreach (var organization in organizations)
        {
            if (organization.ParentId.HasValue && organizationsDictionary.TryGetValue(organization.ParentId.Value, out var parent))
            {
                parent.Children.Add(organization);
            }
        }

        return organizations.Where(o => !o.ParentId.HasValue).ToList();
    }
}
