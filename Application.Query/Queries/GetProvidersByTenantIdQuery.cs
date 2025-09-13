using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Providers;
using Domain.Core.Enums;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries;

public class GetProvidersByTenantIdQuery : IRequest<List<GetProviderVm>>
{
    public int TenantId { get; set; }
    public ProviderType? Type { get; set; }
    public GetProvidersByTenantIdQuery(ProviderType? type, int tenantId)
    {
        TenantId = tenantId;
        Type= type;
    }
}

public class GetProviderByTenantIdQueryHandler : BaseQueryHandler, IRequestHandler<GetProvidersByTenantIdQuery, List<GetProviderVm>>
{
    private readonly ITenantPlatformContractReadOnlyRepository _tenantPlatformContractReadOnlyRepository;
    public GetProviderByTenantIdQueryHandler(ITenantPlatformContractReadOnlyRepository tenantPlatformContractReadOnlyRepository)
    {
        _tenantPlatformContractReadOnlyRepository = tenantPlatformContractReadOnlyRepository;
    }

    public async Task<List<GetProviderVm>> Handle(GetProvidersByTenantIdQuery request, CancellationToken cancellationToken)
    {
        var providers = await _tenantPlatformContractReadOnlyRepository.GetProvidersByTenantIdAsync(request.Type, request.TenantId);

        var result = providers.Select(c => new GetProviderVm
        {
            Description = c.Description,
            EnglishName = c.EnglishName,
            Name = c.Name,
            Id = c.Id,
            ProviderType = c.ProviderType,
        }).ToList();
        return result;
    }
}
