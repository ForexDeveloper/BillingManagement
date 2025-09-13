using Application.Query.ReadOnlyRepositoryContracts;
using Application.Service.Contracts;
using Domain.Core.Helper;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class GetWalletContractEndorsementsByRootParentIdQuery : IRequest<List<WalletContractEndorsementVm>>
    {
        public int Id { get; }
        public int? TenantId { get; }
        public GetWalletContractEndorsementsByRootParentIdQuery(int id, int? tenantId = null)
        {
            Id = id;
            TenantId = tenantId;
        }
    }

    public class GetWalletContractEndorsementsByRootParentIdQueryHandler : IRequestHandler<GetWalletContractEndorsementsByRootParentIdQuery, List<WalletContractEndorsementVm>>
    {
        private readonly IWalletContractReadOnlyRepository _walletContractReadOnlyRepository;
        private readonly IWalletContractService _walletContractService;

        public GetWalletContractEndorsementsByRootParentIdQueryHandler(IWalletContractReadOnlyRepository walletContractReadOnlyRepository,
            IWalletContractService walletContractService)
        {
            _walletContractReadOnlyRepository = walletContractReadOnlyRepository;
            _walletContractService = walletContractService;
        }

        public async Task<List<WalletContractEndorsementVm>> Handle(GetWalletContractEndorsementsByRootParentIdQuery request, CancellationToken cancellationToken)
        {
            var endorsements = await _walletContractReadOnlyRepository.GetEndorsementsByRootParentIdListAsync(request.Id, request.TenantId);

            var result = endorsements.OrderByDescending(x => x.Id).Select(x => new WalletContractEndorsementVm()
            {
                Id = x.Id,
                ContractNumber = x.ContractNumber,
                Status = x.Status,
                StatusTitle = x.Status.GetEnumDescription(),
                EndDate = x.EndDate,
                DisplayEndDate = _walletContractService.CreateWalletContractDisplayEndDate(x.EndDate),
                ChangeStatusDate = x.ChangeStatusDate
            }).ToList();


            return result;
        }

    }
}
