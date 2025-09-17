using Domain.Core.Entities;
using Domain.Core.Entities.TenantPlatformContractAggregate;
using Domain.Core.Entities.TenantPlatformContractAggregate.Exceptions;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Command.TenantPlatformContractCommands
{
    public class UpdateTenantPlatformContractStatusCommand : IRequest<int>
    {
        public int Id { get; set; }
        public bool Status { get; set; }
        public int? TenantId { get; set; }
        public UpdateTenantPlatformContractStatusCommand(int id, bool status, int? tenantId = null)
        {
            Id = id;
            Status = status;
            TenantId = tenantId;
        }
    }

    public class UpdateTenantPlatformContractStatusCommandHandler : IRequestHandler<UpdateTenantPlatformContractStatusCommand, int>
    {
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly ITenantPlatformContractRepository _tenantPlatformContractRepository;


        public UpdateTenantPlatformContractStatusCommandHandler(
            IApplicationDbContextUnitOfWork unitOfWork, ITenantPlatformContractRepository tenantPlatformContractRepository)
        {
            _unitOfWork = unitOfWork;
            _tenantPlatformContractRepository = tenantPlatformContractRepository;
        }

        public async Task<int> Handle(UpdateTenantPlatformContractStatusCommand request, CancellationToken cancellationToken)
        {
            var contract = await _tenantPlatformContractRepository.GetAsync(request.Id)
                           ?? throw new TenantPlatformContractNotFoundException("قرارداد پیدا نشد.");

            if (request.TenantId.HasValue && request.TenantId != contract.TenantId)
            {
                throw new TenantForbiddenException();
            }

            var hasAppendix = await _tenantPlatformContractRepository.HasEndorsement(contract.Id, contract.TenantId);
            if (hasAppendix)
            {
                throw new TenantPlatformContractNotEditableException("تغییر وضعیت این قرارداد به دلیل وجود الحاقیه ممکن نیست.");
            }

            var isExistsActiveContract = await _tenantPlatformContractRepository.IsExistsActiveContractAsync(contract.TenantId, request.Id);
            if (isExistsActiveContract)
            {
                throw new TenantPlatformContractNotEditableException(" فعال کردن این قرارداد به دلیل وجود قرارداد فعال دیگر، ممکن نیست.");
            }

            if (request.Status)
            {
                if (contract.Status)
                {
                    throw new TenantPlatformContractStatusException("قرارداد قبلا فعال شده است.");
                }

                contract.SetStatus(request.Status);
            }
            else
            {
                if (!contract.Status)
                {
                    throw new TenantPlatformContractStatusException("قرارداد قبلا غیر فعال شده است.");
                }

                contract.SetStatus(request.Status);
            }

            contract.SetEditDateTime(DateTime.Now);

            _tenantPlatformContractRepository.Update(contract);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return contract.Id;
        }

    }
}
