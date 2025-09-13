using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Domain.Core.Enums;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetActiveWalletMerchantListQuery : IRequest<List<int>>
    {
        public GetActiveWalletMerchantListQuery(int customerId, List<int> categories,
            List<int> wallets, SaleType? saleType, string searchValue ,int tenantId)
        {
            CustomerId = customerId;
            Categories = categories;
            Wallets = wallets;
            SaleType = saleType;
            SearchValue = searchValue;
            TenantId = tenantId;
        }

        public int CustomerId { get; }
        public List<int> Categories { get; }
        public List<int> Wallets { get; }
        public SaleType? SaleType { get; }
        public string SearchValue { get; set; }
        public int TenantId { get; set; }

    }
    public class GetActiveWalletMerchantListQueryHandler : BaseQueryHandler, IRequestHandler<GetActiveWalletMerchantListQuery, List<int>>
    {
        private readonly IWalletReadOnlyRepository _walletReadOnlyRepository;
        private readonly IMerchantReadOnlyRepository _merchantReadOnlyRepository;

        public GetActiveWalletMerchantListQueryHandler(IWalletReadOnlyRepository walletReadOnlyRepository, IMerchantReadOnlyRepository merchantReadOnlyRepository)
        {
            _walletReadOnlyRepository = walletReadOnlyRepository;
            _merchantReadOnlyRepository = merchantReadOnlyRepository;
        }

        public async Task<List<int>> Handle(GetActiveWalletMerchantListQuery request, CancellationToken cancellationToken)
        {
            if (request.Wallets is null || !request.Wallets.Any())
                return await _merchantReadOnlyRepository.GetMerchantByFilterAsync(request);

            if (request.Wallets != null && request.Wallets.Any())
            {
                var hasCashWallet = await _walletReadOnlyRepository.HasCashWalletByIdAsync(request.Wallets, request.TenantId);
                if (hasCashWallet)
                    return await _merchantReadOnlyRepository.GetMerchantByFilterAsync(request);
            }

            return await _walletReadOnlyRepository.GetActiveWalletMerchantByFilterAsync(request);
        }
    }
}
