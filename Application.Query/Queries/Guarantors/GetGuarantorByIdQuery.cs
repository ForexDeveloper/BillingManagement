using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Guarantors;
using Domain.Core.Entities.GuarantorAggregate.Exceptions;
using Domain.Core.Enums;
using Domain.Core.Helper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries.Guarantors;

public class GetGuarantorByIdQuery : IRequest<GetGuarantorVm>
{
    public int Id { get; }
    public int? TenantId { get; set; }

    public GetGuarantorByIdQuery(int id, int? tenantId = null)
    {
        Id = id;
        TenantId = tenantId;
    }
}

public class GetGuarantorByIdQueryHandler : BaseQueryHandler, IRequestHandler<GetGuarantorByIdQuery, GetGuarantorVm>
{
    private readonly IGuarantorReadOnlyRepository _guarantorReadOnlyRepository;

    public GetGuarantorByIdQueryHandler(IGuarantorReadOnlyRepository guarantorReadOnlyRepository)
    {
        _guarantorReadOnlyRepository = guarantorReadOnlyRepository;
    }

    public async Task<GetGuarantorVm> Handle(GetGuarantorByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _guarantorReadOnlyRepository.GetAsync(request.Id, request.TenantId);
        if (result == null)
            throw new GuarantorNotFoundException("ضامن پیدا نشد.");

        return new GetGuarantorVm
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
