using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Financiers;
using Domain.Core.Entities.FinancierAggregate.Exceptions;
using Domain.Core.Enums;
using Domain.Core.Helper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries.Financiers;

public class GetFinancierByIdQuery : IRequest<GetFinancierVm>
{
    public int Id { get; }
    public int? TenantId { get; set; }
    public GetFinancierByIdQuery(int id, int? tenantId = null)
    {
        Id = id;
        TenantId = tenantId;
    }
}

public class GetFinancierByIdQueryHandler : BaseQueryHandler, IRequestHandler<GetFinancierByIdQuery, GetFinancierVm>
{
    private readonly IFinancierReadOnlyRepository _financierReadOnlyRepository;
    public GetFinancierByIdQueryHandler(IFinancierReadOnlyRepository financierReadOnlyRepository)
    {
        _financierReadOnlyRepository = financierReadOnlyRepository;
    }

    public async Task<GetFinancierVm> Handle(GetFinancierByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _financierReadOnlyRepository.GetAsync(request.Id, request.TenantId);
        if (result == null)
            throw new FinancierNotFoundException("تامین کننده مالی پیدا نشد.");

        return new GetFinancierVm
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
