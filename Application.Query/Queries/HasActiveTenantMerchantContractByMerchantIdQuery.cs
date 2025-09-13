using Application.Query.ReadOnlyRepositoryContracts;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries
{
    public class HasActiveTenantMerchantContractByMerchantIdQuery : IRequest<bool>
    {
        public int Id { get; }
        public int TenantId { get; }
        public HasActiveTenantMerchantContractByMerchantIdQuery(int id, int tenantId)
        {
            Id = id;
            TenantId = tenantId;
        }
    }

    public class HasActiveTenantMerchantContractByMerchantIdQueryHandler : IRequestHandler<HasActiveTenantMerchantContractByMerchantIdQuery, bool>
    {
        private readonly ITenantMerchantContractReadOnlyRepository _tenantMerchantContractReadOnlyRepository;

        public HasActiveTenantMerchantContractByMerchantIdQueryHandler(ITenantMerchantContractReadOnlyRepository tenantMerchantContractReadOnlyRepository)
        {
            _tenantMerchantContractReadOnlyRepository = tenantMerchantContractReadOnlyRepository;
        }

        public async Task<bool> Handle(HasActiveTenantMerchantContractByMerchantIdQuery request, CancellationToken cancellationToken)
        {
            var IsExistactiveTenantMerchantContract = await _tenantMerchantContractReadOnlyRepository.GetActiveContractAsync(request.TenantId, request.Id);

            return IsExistactiveTenantMerchantContract;
        }
    }
}
