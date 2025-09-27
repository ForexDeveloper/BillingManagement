using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels;
using Application.Query.ViewModels.Providers;
using Domain.Core.Enums;
using Domain.Core.Helper;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries.Providers;

public class GetProvidersQuery : BasePaginatedListRequest, IRequest<GetProvidersVm>
{
    public ProviderType? ProviderType { get; set; }
    public GetProvidersQuery(ProviderType? providerType, int pageIndex, int pageSize, string sortColumn, SortDirection? sortDirection, string searchValue)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        SortColumn = sortColumn;
        SortDirection = sortDirection;
        SearchValue = searchValue;
        ProviderType = providerType;
    }

    public class GetProvidersQueryHandler : BaseQueryHandler, IRequestHandler<GetProvidersQuery, GetProvidersVm>
    {
        private readonly IProviderReadOnlyRepository _providerReadOnlyRepository;

        public GetProvidersQueryHandler(IProviderReadOnlyRepository providerReadOnlyRepository)
        {
            _providerReadOnlyRepository = providerReadOnlyRepository;
        }


        public async Task<GetProvidersVm> Handle(GetProvidersQuery request, CancellationToken cancellationToken)
        {
            var providers = await _providerReadOnlyRepository.GetProvidersAsync(request);

            return new GetProvidersVm()
            {
                PageIndex = providers.PageIndex,
                PageSize = providers.PageSize,
                TotalCount = providers.TotalCount,
                Items = providers.Items.Select(x =>
                new GetProviderVm
                {
                    Id = x.Id,
                    ProviderType = x.ProviderType,
                    ProviderTypeName = ((ProviderType)Enum.Parse(typeof(ProviderType), x.ProviderType.ToString())).GetEnumDescription(),
                    Name = x.Name,
                    Description = x.Description,
                    EnglishName=x.EnglishName,
                }
                ).ToList()
            };
        }
    }
}
