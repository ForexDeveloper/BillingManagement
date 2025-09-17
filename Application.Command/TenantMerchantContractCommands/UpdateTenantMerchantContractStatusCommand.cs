using Domain.Core.Entities;
using Domain.Core.Entities.TenantMerchantContractAggregate;
using Domain.Core.Entities.TenantMerchantContractAggregate.Exceptions;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Command.TenantMerchantContractCommands
{
    public class UpdateTenantMerchantContractStatusCommand : IRequest<int>
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }
        public bool Status { get; set; }

        public UpdateTenantMerchantContractStatusCommand(int id, bool status, int? tenantId = null)
        {
            Id = id;
            TenantId = tenantId;
            Status = status;
        }
    }

    public class UpdateTenantMerchantContractStatusCommandHandler : IRequestHandler<UpdateTenantMerchantContractStatusCommand, int>
    {
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly ITenantMerchantContractRepository _tenantMerchantContractRepository;


        public UpdateTenantMerchantContractStatusCommandHandler(
            IApplicationDbContextUnitOfWork unitOfWork, ITenantMerchantContractRepository tenantMerchantContractRepository)
        {
            _unitOfWork = unitOfWork;
            _tenantMerchantContractRepository = tenantMerchantContractRepository;
        }

        public async Task<int> Handle(UpdateTenantMerchantContractStatusCommand request, CancellationToken cancellationToken)
        {
            var contract = await _tenantMerchantContractRepository.GetAsync(request.Id)
                ?? throw new TenantMerchantContractNotFoundException("قرارداد پیدا نشد.");

            if (request.TenantId.HasValue && request.TenantId != contract.TenantId)
            {
                throw new TenantForbiddenException();
            }

            var hasEndorsement = await _tenantMerchantContractRepository.HasEndorsement(contract.Id, contract.TenantId);
            if (hasEndorsement)
            {
                throw new TenantMerchantContractNotEditableException("تغییر وضعیت این قرارداد به دلیل وجود الحاقیه ممکن نیست.");
            }

            var isExistsActiveContract = await _tenantMerchantContractRepository.IsExistsActiveContractAsync(contract.TenantId, contract.MerchantId, request.Id);
            if (isExistsActiveContract)
            {
                throw new TenantMerchantContractNotEditableException(" فعال کردن این قرارداد به دلیل وجود قرارداد فعال دیگر، ممکن نیست.");
            }


            if (request.Status)
            {
                if (contract.Status)
                {
                    throw new TenantMerchantContractStatusException("قرارداد قبلا فعال شده است.");
                }

                contract.SetStatus(request.Status);
            }
            else
            {
                if (!contract.Status)
                {
                    throw new TenantMerchantContractStatusException("قرارداد قبلا غیر فعال شده است.");
                }

                contract.SetStatus(request.Status);
            }

            contract.SetEditDateTime(DateTime.Now);

            _tenantMerchantContractRepository.Update(contract);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return contract.Id;
        }

    }
}
