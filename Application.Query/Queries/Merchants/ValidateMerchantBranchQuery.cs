using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Domain.Core.Entities.MerchantAggregate.Exceptions;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantMerchantContractAggregate.Exceptions;
using Domain.Core.Enums;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries.Merchants;

public class ValidateMerchantBranchQuery : IRequest<bool>
{
    public long BranchTerminalId { get; set; }

    public ValidateMerchantBranchQuery(long branchTerminalId)
    {
        BranchTerminalId = branchTerminalId;
    }

    public class ValidateMerchantBranchQueryHandler : BaseQueryHandler, IRequestHandler<ValidateMerchantBranchQuery, bool>
    {
        private readonly IMerchantReadOnlyRepository _merchantReadOnlyRepository;
        private readonly ITenantMerchantContractReadOnlyRepository _tenantMerchantContractReadOnlyRepository;

        public ValidateMerchantBranchQueryHandler(IMerchantReadOnlyRepository merchantReadOnlyRepository,
            ITenantMerchantContractReadOnlyRepository tenantMerchantContractReadOnlyRepository)
        {
            _merchantReadOnlyRepository = merchantReadOnlyRepository;
            _tenantMerchantContractReadOnlyRepository = tenantMerchantContractReadOnlyRepository;
        }

        public async Task<bool> Handle(ValidateMerchantBranchQuery request, CancellationToken cancellationToken)
        {
            var merchantBranch = await _merchantReadOnlyRepository.GetMerchantBranchByTerminalIdAsync(request.BranchTerminalId);

            if (merchantBranch == null)
                throw new MerchantNotFoundException("شعبه پذیرنده وجود ندارد.");

            if (merchantBranch.Merchant.Status != MerchantStatus.Active)
                throw new ArgumentValidationException("MerchantId", "پذیرنده فعال نیست.");

            var contractExists = await _tenantMerchantContractReadOnlyRepository.GetActiveContractAsync(merchantBranch.Merchant.TenantId, merchantBranch.MerchantId);

            if (!contractExists)
                throw new TenantMerchantContractStatusException("قرارداد پذیرنده و مالک زیرساخت فعال نیست.");

            return true;
        }
    }
}
