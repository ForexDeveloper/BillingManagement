using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Providers;
using Domain.Core.Entities.Providers.Exceptions;
using Domain.Core.Enums;
using Domain.Core.Helper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries;

public class GetProviderByIdQuery : IRequest<GetProviderVm>
{
    public int Id { get; }
    public GetProviderByIdQuery(int id)
    {
        Id = id;
    }
}

public class GetProviderByIdQueryHandler : BaseQueryHandler, IRequestHandler<GetProviderByIdQuery, GetProviderVm>
{
    private readonly IProviderReadOnlyRepository _providerReadOnlyRepository;

    public GetProviderByIdQueryHandler(IProviderReadOnlyRepository providerReadOnlyRepository)
    {
        _providerReadOnlyRepository = providerReadOnlyRepository;
    }

    public async Task<GetProviderVm> Handle(GetProviderByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _providerReadOnlyRepository.GetAsync(request.Id);
        if (result == null)
            throw new ProviderNotFoundException("سرویس مورد نظر پیدا نشد.");

        return new GetProviderVm
        {
            Id = result.Id,
            ProviderType = result.ProviderType,
            ProviderTypeName = ((ProviderType)Enum.Parse(typeof(ProviderType), result.ProviderType.ToString())).GetEnumDescription(),
            Name = result.Name,
            Description = result.Description,
            EnglishName=result.EnglishName, 
        };
    }
}
