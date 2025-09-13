using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.WalletConfigurations;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetAllWalletConfigurationWithOutCashWalletQuery : IRequest<List<GetWalletConfigurationWithOutCashWalletViewModel>>
    {
        public int? TenantId { get; set; }

        public GetAllWalletConfigurationWithOutCashWalletQuery( int? tenantId)
        {
            TenantId = tenantId;
        }
    }

    public class AllWalletConfigurationWithOutCashWalletQueryHandler : BaseQueryHandler, IRequestHandler<GetAllWalletConfigurationWithOutCashWalletQuery, List<GetWalletConfigurationWithOutCashWalletViewModel>>
    {
        private readonly IWalletConfigurationReadOnlyRepository _WalletConfigurationReadOnlyRepository;
        public AllWalletConfigurationWithOutCashWalletQueryHandler(IWalletConfigurationReadOnlyRepository WalletConfigurationReadOnlyRepository)
        {
            _WalletConfigurationReadOnlyRepository = WalletConfigurationReadOnlyRepository;
        }

        public async Task<List<GetWalletConfigurationWithOutCashWalletViewModel>> Handle(GetAllWalletConfigurationWithOutCashWalletQuery query, CancellationToken cancellationToken)
        {
            var walletConfigurations = await _WalletConfigurationReadOnlyRepository.GetListWithOutCashWalletAsync(query.TenantId);
            var result = walletConfigurations.Select(x => new GetWalletConfigurationWithOutCashWalletViewModel
            {
                Id = x.Id,
                Title = x.Title,
            }).ToList();

           return result;
        }
    }
}