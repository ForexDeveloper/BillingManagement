using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Facilitators;
using Domain.Core.Entities.FacilitatorAggregate.Exceptions;
using Domain.Core.Enums;
using Domain.Core.Helper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries;

public class GetFacilitatorByIdQuery : IRequest<GetFacilitatorVm>
{
    public int Id { get; }
    public int? TenantId { get; set; }

    public GetFacilitatorByIdQuery(int id, int? tenantId = null)
    {
        Id = id;
        TenantId = tenantId;
    }
}

public class GetFacilitatorByIdQueryHandler : BaseQueryHandler, IRequestHandler<GetFacilitatorByIdQuery, GetFacilitatorVm>
{
    private readonly IFacilitatorReadOnlyRepository _facilitatorReadOnlyRepository;
    public GetFacilitatorByIdQueryHandler(IFacilitatorReadOnlyRepository facilitatorReadOnlyRepository)
    {
        _facilitatorReadOnlyRepository = facilitatorReadOnlyRepository;
    }

    public async Task<GetFacilitatorVm> Handle(GetFacilitatorByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _facilitatorReadOnlyRepository.GetAsync(request.Id, request.TenantId);
        if (result == null)
            throw new FacilitatorNotFoundException("تسهیلگر پیدا نشد.");

        return new GetFacilitatorVm
        {
            Id = result.Id,
            Name = result.Name,
            TenantId = result.TenantId,
            TenantName = result.Tenant.Title,
            PersonType = (IdentityTypeEnum)Enum.Parse(typeof(IdentityTypeEnum), result.Type.ToString()),
            PersonTypeName = ((IdentityTypeEnum)Enum.Parse(typeof(IdentityTypeEnum), result.Type.ToString())).GetEnumDescription(),
        };
    }
}
