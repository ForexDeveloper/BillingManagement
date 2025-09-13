using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Wallets;
using MediatR;
using Shared.IdentityServerProvider.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetActiveWalletCategoryListQuery : IRequest<List<GetActiveWalletCategoryListViewModel>>
    {
        public GetActiveWalletCategoryListQuery(int customerId, int? walletId)
        {
            CustomerId = customerId;
            WalletId = walletId;
        }
        public int CustomerId { get; }
        public int? WalletId { get; }

    }
    public class GetActiveWalletCategoryListQueryHandler : BaseQueryHandler, IRequestHandler<GetActiveWalletCategoryListQuery, List<GetActiveWalletCategoryListViewModel>>
    {
        private readonly IWalletReadOnlyRepository _walletReadOnlyRepository;
        private readonly IMerchantReadOnlyRepository _merchantReadOnlyRepository;
        private readonly ICurrentUserService _currentUserService;
        public GetActiveWalletCategoryListQueryHandler(IWalletReadOnlyRepository walletReadOnlyRepository, IMerchantReadOnlyRepository merchantReadOnlyRepository, ICurrentUserService currentUserService)
        {
            _walletReadOnlyRepository = walletReadOnlyRepository;
            _merchantReadOnlyRepository = merchantReadOnlyRepository;
            _currentUserService = currentUserService;
        }

        public async Task<List<GetActiveWalletCategoryListViewModel>> Handle(GetActiveWalletCategoryListQuery request, CancellationToken cancellationToken)
        {
            if (request.WalletId.HasValue)
            {
                var wallet = await _walletReadOnlyRepository.GetWalletByCustomerIdAsync(request.WalletId.Value, request.CustomerId);
                if (wallet is null)
                    return new List<GetActiveWalletCategoryListViewModel>();

                if (wallet.WalletTypeId == Domain.Core.Enums.WalletType.Cash)
                {
                    return await GetCategoryByTenantIdAsync(_currentUserService.TenantId);
                }
                else
                {
                    return await GetActiveWalletCategoryListAsync(request.WalletId.Value, request.CustomerId);
                }
            }
            var hasCashWallet = await _walletReadOnlyRepository.HasCashWalletByIdAsync( request.CustomerId, _currentUserService.TenantId);
            if (hasCashWallet)
            {
                return await GetCategoryByTenantIdAsync(_currentUserService.TenantId);
            }
            return await GetActiveWalletCategoryListAsync(request.WalletId, request.CustomerId);
       
        }
        private async Task<List<GetActiveWalletCategoryListViewModel>> GetActiveWalletCategoryListAsync(int? walletId, int customerId)
        {
            var categories = await _walletReadOnlyRepository.GetActiveWalletCategoryListAsync(walletId, customerId);
            var categoryModel = categories.Select(c => new GetActiveWalletCategoryListViewModel
            {
                Id = c.Id,
                Title = c.Title,
            }).OrderBy(c => c.Id).ToList();

            return categoryModel;
        }
        private async Task<List<GetActiveWalletCategoryListViewModel>> GetCategoryByTenantIdAsync(int tenantId)
        {
            var categories = await _merchantReadOnlyRepository.GetCategoryByTenantIdAsync(tenantId);
            var categoryModel = categories.Select(c => new GetActiveWalletCategoryListViewModel
            {
                Id = c.Id,
                Title = c.Title,
            }).OrderBy(c => c.Id).ToList();

            return categoryModel;
        }
    }
}
