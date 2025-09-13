using Application.Query.ReadOnlyRepositoryContracts;
using Domain.Core.Entities.TenantMerchantContractAggregate.Exceptions;
using Domain.Core.Entities.TenantPlatformContractAggregate.Exceptions;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Entities.WalletContractAggregate.Exceptions;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Command.WalletContractCommands
{
    public class UpdateWalletContractAcceptanceStatusCommand : IRequest<int>
    {
        public int Id { get; set; }
        public int? TenantId { get; set; }
        public bool Status { get; set; }
        public string? Reason { get; set; }

        public UpdateWalletContractAcceptanceStatusCommand(int id, bool status, string? reason = null, int? tenantId = null)
        {
            Id = id;
            TenantId = tenantId;
            Status = status;
            Reason = reason;
        }
    }

    public class UpdateWalletContractAcceptanceStatusCommandHandler : IRequestHandler<UpdateWalletContractAcceptanceStatusCommand, int>
    {
        private readonly IWalletContractRepository _walletContractRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly IWalletContractReadOnlyRepository _walletContractReadOnlyRepository;

        public UpdateWalletContractAcceptanceStatusCommandHandler(IWalletContractRepository walletContractRepository,
            IApplicationDbContextUnitOfWork unitOfWork,
            IWalletContractReadOnlyRepository walletContractReadOnlyRepository = null)
        {
            _walletContractRepository = walletContractRepository;
            _unitOfWork = unitOfWork;
            _walletContractReadOnlyRepository = walletContractReadOnlyRepository;
        }

        public async Task<int> Handle(UpdateWalletContractAcceptanceStatusCommand request, CancellationToken cancellationToken)
        {

            var contract = await _walletContractRepository.GetAsync(request.Id, request.TenantId)
                           ?? throw new WalletContractNotFoundException("قرارداد پیدا نشد.");

            if (contract.GrantingProcessId.HasValue)
            {
                throw new TenantPlatformContractNotEditableException("تغییر وضعیت این قرارداد ممکن نیست.");
            }

            var hasCashWallet = await _walletContractReadOnlyRepository.HasCashWalletContractByIdAsync(request.Id);
            if (hasCashWallet)
            {
                throw new WalletContractNotEditableException("تغییر وضعیت این قرارداد ممکن نیست.");
            }

            if (contract.Status == WalletContractStatus.Active || contract.Status == WalletContractStatus.DeActive)
                throw new WalletContractAcceptanceStatusException("فقط قراردادهایی با وضعیت رد شده یا در حال بررسی امکان تایید و یا رد شدن را دارند.");

            var existsActiveOrDeactiveContract = await _walletContractReadOnlyRepository.ExistsActiveOrDeactiveContractWithIdGreaterThan(contract.Id, contract.RootParentId, contract.TenantId);
            if (existsActiveOrDeactiveContract)
                throw new WalletContractNotEditableException("تغییر وضعیت این قرارداد ممکن نیست.");

            if (request.Status)
            {
                contract.SetStatus(WalletContractStatus.DeActive);
                contract.SetChangeStatusDate();
            }
            else
            {
                contract.SetStatus(WalletContractStatus.Reject);
                if (!string.IsNullOrEmpty(request.Reason))
                {
                    contract.AddWalletContractRejectionReasons(request.Reason);
                }
                contract.SetChangeStatusDate();
            }

            contract.SetEditDateTime(DateTime.Now);
            _walletContractRepository.Update(contract);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return contract.Id;
        }
    }
}
