using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Organizations;
using Domain.Core.Entities.OrganizationAggregate.Exceptions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries.Organizations;

public class GetOrganizationByIdQuery : IRequest<GetOrganizationVm>
{
    public int Id { get; }
    public int? TenantId { get; }
    public GetOrganizationByIdQuery(int id, int? tenantId = null)
    {
        Id = id;
        TenantId = tenantId;
    }
}

public class GetOrganizationByIdQueryHandler : BaseQueryHandler, IRequestHandler<GetOrganizationByIdQuery, GetOrganizationVm>
{
    private readonly IOrganizationReadOnlyRepository _organizationReadOnlyRepository;
    public GetOrganizationByIdQueryHandler(IOrganizationReadOnlyRepository organizationReadOnlyRepository)
    {
        _organizationReadOnlyRepository = organizationReadOnlyRepository;
    }

    public async Task<GetOrganizationVm> Handle(GetOrganizationByIdQuery request, CancellationToken cancellationToken)
    {
        var organization = await _organizationReadOnlyRepository.GetByIdAsync(request.Id, request.TenantId);

        if (organization == null)
            throw new OrganizationNotFoundException("سازمان پیدا نشد.");

        return new GetOrganizationVm
        {
            Id = organization.Id,
            TenantId = organization.TenantId,
            Title = organization.Title,
            ParentId = organization.ParentId,
            ParentName = organization.ParentName,
            TenantName = organization.TenantName,
        };
    }
}