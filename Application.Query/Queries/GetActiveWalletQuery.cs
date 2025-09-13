using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Wallets;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetActiveWalletQuery : IRequest<List<GetActiveWalletViewModel>>
    {
        public GetActiveWalletQuery(int customerId)
        {
            CustomerId = customerId;
        }
        public int CustomerId { get; }
    }
    public class GetActiveWalletQueryHandler : BaseQueryHandler, IRequestHandler<GetActiveWalletQuery, List<GetActiveWalletViewModel>>
    {
        private readonly IWalletReadOnlyRepository _walletReadOnlyRepository;
        public GetActiveWalletQueryHandler(IWalletReadOnlyRepository walletReadOnlyRepository)
        {
            _walletReadOnlyRepository = walletReadOnlyRepository;
        }

        public async Task<List<GetActiveWalletViewModel>> Handle(GetActiveWalletQuery request, CancellationToken cancellationToken)
        {
            var wallets = await _walletReadOnlyRepository.GetActiveWalletsAsync(request.CustomerId);
            var result = wallets.Select(c => new GetActiveWalletViewModel
            {
                Balance = c.Balance,
                Type = c.WalletType,
                Id = c.Id,
                Title = c.Title,
            }).ToList();
            return result;
        }
    }
}
