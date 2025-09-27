using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Facilitators;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries.Facilitators;
public class GetFacilitatorsLookupQuery : IRequest<List<LookupItemVm>>
{
    public GetFacilitatorsLookupQuery(int tenantId)
    {
        TenantId = tenantId;
    }
    public int TenantId { get; private set; }
}
public class GetFacilitatorsLookupQueryHandler : BaseQueryHandler, IRequestHandler<GetFacilitatorsLookupQuery, List<LookupItemVm>>
{
    private readonly IFacilitatorReadOnlyRepository _facilitatorReadOnlyRepository;

    public GetFacilitatorsLookupQueryHandler(IFacilitatorReadOnlyRepository facilitatorReadOnlyRepository)
    {
        _facilitatorReadOnlyRepository = facilitatorReadOnlyRepository;
    }

    public async Task<List<LookupItemVm>> Handle(GetFacilitatorsLookupQuery request, CancellationToken cancellationToken)
    {
        var facilitatorsModel = await _facilitatorReadOnlyRepository.GetFacilitatorsLookupAsync(request);

        return facilitatorsModel
            .Select(x => new LookupItemVm
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
    }
}