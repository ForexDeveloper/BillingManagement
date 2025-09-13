using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetCustomerWalletBalanceQuery : IRequest<decimal>
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public int TenantId { get; set; }

        public GetCustomerWalletBalanceQuery(int id, int walletId, int tenantId)
        {
            Id = id;
            WalletId = walletId;
            TenantId = tenantId;
        }
    }

    public class GetCustomerWalletBalanceQueryHandler : BaseQueryHandler, IRequestHandler<GetCustomerWalletBalanceQuery, decimal>
    {
        private readonly IWalletReadOnlyRepository _walletReadOnlyRepository;

        public GetCustomerWalletBalanceQueryHandler(IWalletReadOnlyRepository walletReadOnlyRepository)
        {
            _walletReadOnlyRepository = walletReadOnlyRepository;
        }

        public async Task<decimal> Handle(GetCustomerWalletBalanceQuery request, CancellationToken cancellationToken)
        {
            var balance = await _walletReadOnlyRepository.GetCustomerWalletBalance(request.TenantId, request.Id, request.WalletId, cancellationToken);

            return balance;
        }
    }
}
