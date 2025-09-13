using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Plans;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetPlanSimpleListQuery : IRequest<List<PlanSimpleListViewModel>>
    {
        public int TenantId { get; set; }

        public GetPlanSimpleListQuery(int tenantId)
        {
            TenantId = tenantId;
        }
    }

    public class PlanSimpleListQueryHandler : BaseQueryHandler, IRequestHandler<GetPlanSimpleListQuery, List<PlanSimpleListViewModel>>
    {
        private readonly IPlanReadOnlyRepository _planReadOnlyRepository;
        public PlanSimpleListQueryHandler(IPlanReadOnlyRepository planReadOnlyRepository)
        {
            _planReadOnlyRepository = planReadOnlyRepository;
        }

        public async Task<List<PlanSimpleListViewModel>> Handle(GetPlanSimpleListQuery query, CancellationToken cancellationToken)
        {
            var result = await _planReadOnlyRepository.GetPlansSimpleList(query.TenantId, cancellationToken);

            return result.Select(p=> new PlanSimpleListViewModel
            {
                Id = p.Id,
                Title = p.Title,
            }).ToList();
        }
    }

    public class GetAllPlanQuery : BasePaginatedListRequest, IRequest<GetPlanForGridViewModel>
    {
        public int TenantId { get; set; }
        public int WalletConfigurationId { get; set; }
        public GetAllPlanQuery(BasePaginatedListRequest query, int tenantId, int walletConfigurationId)
        {
            PageIndex = query.PageIndex;
            PageSize = query.PageSize;
            SortColumn = query.SortColumn;
            SortDirection = query.SortDirection;
            SearchValue = query.SearchValue;
            TenantId = tenantId;
            WalletConfigurationId = walletConfigurationId;
        }
    }

    public class PlanQueryHandler : BaseQueryHandler, IRequestHandler<GetAllPlanQuery, GetPlanForGridViewModel>
    {
        private readonly IPlanReadOnlyRepository _planReadOnlyRepository;
        public PlanQueryHandler(IPlanReadOnlyRepository planReadOnlyRepository)
        {
            _planReadOnlyRepository = planReadOnlyRepository;
        }

        public async Task<GetPlanForGridViewModel> Handle(GetAllPlanQuery query, CancellationToken cancellationToken)
        {
            var result = await _planReadOnlyRepository.GetListAsync(query);

            return new GetPlanForGridViewModel()
            {
                PageIndex = result.PageIndex,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                Items = result.Items.Select(x => new GetAllPlanViewModel()
                {
                    Id = x.Id,
                    Title = x.Title,
                    WalletTypeId = x.WalletTypeId,
                    TenantId = x.TenantId,
                    WalletConfigurationTitle = x.WalletConfigurationTitle,
                    TenantTitle = x.TenantTitle,
                    MaxTotalCredit = x.MaxTotalCredit,
                    MaxWallet = x.MaxWallet,
                    TermsAndConditions = x.TermsAndConditions,
                    MaxDailyWithdrawal = x.MaxDailyWithdrawal,
                    MaxDailyTransactionCount = x.MaxDailyTransactionCount,
                    MaxDailyDeposit = x.MaxDailyDeposit,
                }).ToList()
            };
        }
    }
}