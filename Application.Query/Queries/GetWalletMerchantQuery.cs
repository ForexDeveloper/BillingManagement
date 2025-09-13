using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Wallets;
using Domain.Core.Enums;
using MediatR;
using Shared.IdentityServerProvider.Contracts;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetWalletMerchantQuery : IRequest<GetWalletMerchantVm>
    {
        public GetWalletMerchantQuery(int customerId)
        {
            CustomerId = customerId;
        }

        public int CustomerId { get; }
    }

    public class GetWalletMerchantQueryHandler : IRequestHandler<GetWalletMerchantQuery, GetWalletMerchantVm>
    {
        private readonly IWalletReadOnlyRepository _walletReadOnlyRepository;

        public GetWalletMerchantQueryHandler(ICurrentUserService currentUserService
            , IWalletReadOnlyRepository walletReadOnlyRepository)
        {
            _walletReadOnlyRepository = walletReadOnlyRepository;
        }
        public async Task<GetWalletMerchantVm> Handle(GetWalletMerchantQuery request, CancellationToken cancellationToken)
        {
            var walletMerchant = new GetWalletMerchantVm { Type = TempateType.Empty };
            var merchantCount = await _walletReadOnlyRepository.CountActiveWalletMerchantAsync(request.CustomerId);
            if (merchantCount == 0)
            {
                return walletMerchant;
            }
            else if (merchantCount == 1)
            {
                var merchants = await _walletReadOnlyRepository.GetActiveWalletMerchantListAsync(request.CustomerId);
                walletMerchant.MerchantId = merchants.First();
                walletMerchant.Type = TempateType.Single;
                return walletMerchant;
            }
            else
            {
                var merchants = await _walletReadOnlyRepository.GetActiveWalletMerchantWithSaleTypeAsync(request.CustomerId);
                var groupBySaleTypes = merchants.GroupBy(c => c.SaleType);
                if (groupBySaleTypes.Count() == 1 && groupBySaleTypes.Any(c => c.Key == SaleType.InPerson))
                {
                    walletMerchant.Type = TempateType.Multi;
                    return walletMerchant;
                }
                walletMerchant.Type = TempateType.All;
                return walletMerchant;
            }

        }

    }
}
