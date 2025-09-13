using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.WalletConfigurations;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetAllWalletConfigurationQuery : BasePaginatedListRequest, IRequest<GetWalletConfigurationViewModel>
    {
        public int TenantId { get; set; }
        public GetAllWalletConfigurationQuery(BasePaginatedListRequest query ,int tenantId)
        {
            PageIndex = query.PageIndex;
            PageSize = query.PageSize;
            SortColumn = query.SortColumn;
            SortDirection = query. SortDirection;
            SearchValue = query.SearchValue;
            TenantId=tenantId;
        }
    }

    public class WalletConfigurationQueryHandler : BaseQueryHandler, IRequestHandler<GetAllWalletConfigurationQuery, GetWalletConfigurationViewModel>
    {
        private readonly IWalletConfigurationReadOnlyRepository _WalletConfigurationReadOnlyRepository;
        public WalletConfigurationQueryHandler(IWalletConfigurationReadOnlyRepository WalletConfigurationReadOnlyRepository)
        {
            _WalletConfigurationReadOnlyRepository = WalletConfigurationReadOnlyRepository;
        }

        public async Task<GetWalletConfigurationViewModel> Handle(GetAllWalletConfigurationQuery query, CancellationToken cancellationToken)
        {
            var result = await _WalletConfigurationReadOnlyRepository.GetListAsync(query);

            return new GetWalletConfigurationViewModel()
            {
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                Items = result.Items.Select(x =>  new WalletConfigurationsViewModel()
                {
                    Id = x.Id,
                    Title = x.Title,
                    WalletTypeId = x.WalletTypeId,
                    MaxWallet = x.MaxWallet,
                    TenantId = x.TenantId,
                    ProjectManagerId = x.ProjectManagerId,
                    ProjectManagerFullName = x.ProjectManagerFullName,
                    TenantTitle=x.TenantTitle,
                    PlanCount=x.PlanCount,
                }).ToList()
            };
        }
    }
}