using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Closedloops;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetAllClosedloopQuery : BasePaginatedListRequest, IRequest<GetClosedloopForGridViewModel>
    {
        public int TenantId { get; set; }
        public int WalletConfigurationId { get; set; }
        public GetAllClosedloopQuery(BasePaginatedListRequest query ,int tenantId, int walletConfigurationId)
        {
            PageIndex = query.PageIndex;
            PageSize = query.PageSize;
            SortColumn = query.SortColumn;
            SortDirection = query. SortDirection;
            SearchValue = query.SearchValue;
            TenantId=tenantId;
            WalletConfigurationId= walletConfigurationId;
        }
    }

    public class ClosedloopQueryHandler : BaseQueryHandler, IRequestHandler<GetAllClosedloopQuery, GetClosedloopForGridViewModel>
    {
        private readonly IClosedloopReadOnlyRepository _ClosedloopReadOnlyRepository;
        public ClosedloopQueryHandler(IClosedloopReadOnlyRepository ClosedloopReadOnlyRepository)
        {
            _ClosedloopReadOnlyRepository = ClosedloopReadOnlyRepository;
        }

        public async Task<GetClosedloopForGridViewModel> Handle(GetAllClosedloopQuery query, CancellationToken cancellationToken)
        {
            var result = await _ClosedloopReadOnlyRepository.GetListAsync(query);

            return new GetClosedloopForGridViewModel()
            {
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                Items = result.Items.Select(x =>  new GetAllClosedloopViewModel()
                {
                    Id = x.Id,
                    Title = x.Title,
                    WalletConfigurationTitle = x.WalletConfigurationTitle,
                    WalletConfigurationId=x.WalletConfigurationId
                }).ToList()
            };
        }
    }
}