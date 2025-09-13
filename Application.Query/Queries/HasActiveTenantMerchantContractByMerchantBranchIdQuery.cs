using Application.Query.ReadOnlyRepositoryContracts;
using Domain.Core.Entities.MerchantAggregate.Exceptions;
using MediatR;
using Shared.IdentityServerProvider;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class HasActiveTenantMerchantContractByMerchantBranchIdQuery : IRequest<bool>
    {
        public int Id { get; }
        public int TenantId { get; }
        public HasActiveTenantMerchantContractByMerchantBranchIdQuery(int id, int tenantId)
        {
            Id = id;
            TenantId = tenantId;
        }
    }

    public class HasActiveTenantMerchantContractByMerchantBranchIdQueryHandler : IRequestHandler<HasActiveTenantMerchantContractByMerchantBranchIdQuery, bool>
    {
        private readonly ITenantMerchantContractReadOnlyRepository _tenantMerchantContractReadOnlyRepository;
        private readonly IMerchantReadOnlyRepository _merchantReadOnlyRepository;

        public HasActiveTenantMerchantContractByMerchantBranchIdQueryHandler(ITenantMerchantContractReadOnlyRepository tenantMerchantContractReadOnlyRepository, IMerchantReadOnlyRepository merchantReadOnlyRepository)
        {
            _tenantMerchantContractReadOnlyRepository = tenantMerchantContractReadOnlyRepository;
            _merchantReadOnlyRepository = merchantReadOnlyRepository;
        }

        public async Task<bool> Handle(HasActiveTenantMerchantContractByMerchantBranchIdQuery request, CancellationToken cancellationToken)
        {
            var merchantBranch = await _merchantReadOnlyRepository.GetBranchByIdAsync(request.Id) ?? throw new MerchantBranchNotFoundException("شعبه پذیرنده پیدا نشد.");
            if (merchantBranch.Merchant.TenantId != request.TenantId)
            {
                throw new ArgumentValidationException(nameof(request.Id), "شعبه پذیرنده پیدا نشد.");
            }

            var IsExistactiveTenantMerchantContract = await _tenantMerchantContractReadOnlyRepository.GetActiveContractAsync(request.TenantId, merchantBranch.Merchant.Id);

            return IsExistactiveTenantMerchantContract;
        }
    }
}
