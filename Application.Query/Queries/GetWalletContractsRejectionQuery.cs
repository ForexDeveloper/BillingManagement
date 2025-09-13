using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.WalletContracts;
using Domain.Core.Entities.WalletContractAggregate.Exceptions;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetWalletContractsRejectionQuery : IRequest<List<WalletContractRejectionVm>>
    {
        public int WalletContractId { get; set; }
        public int? TenantId { get; set; }

        public GetWalletContractsRejectionQuery(int walletContractId, int? tenantId = null)
        {
            WalletContractId = walletContractId;
            TenantId = tenantId;
        }
    }

    public class GetWalletContractsRejectionQueryHandler : IRequestHandler<GetWalletContractsRejectionQuery, List<WalletContractRejectionVm>>
    {
        private readonly IWalletContractReadOnlyRepository _walletContractReadOnlyRepository;

        public GetWalletContractsRejectionQueryHandler(IWalletContractReadOnlyRepository walletContractReadOnlyRepository)
        {
            _walletContractReadOnlyRepository = walletContractReadOnlyRepository;
        }

        public async Task<List<WalletContractRejectionVm>> Handle(GetWalletContractsRejectionQuery request, CancellationToken cancellationToken)
        {
            if (request.TenantId.HasValue)
            {
                var isWalletContractBelongToTenantAsync = await _walletContractReadOnlyRepository.IsWalletContractBelongToTenantAsync(request.WalletContractId, request.TenantId.Value);
                if (!isWalletContractBelongToTenantAsync)
                {
                    throw new WalletContractInvalidTenantException("قرارداد پیدا نشد.");
                }
            }
            var rejections = await _walletContractReadOnlyRepository.GetRejectionListAsync(request.WalletContractId);

            return rejections.Select(x => new WalletContractRejectionVm(x.Id, x.Reason, x.CreDateTime)).ToList();
        }

    }
}
