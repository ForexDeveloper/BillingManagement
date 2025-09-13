using Application.Query.ReadOnlyRepositoryContracts;
using Domain.Core.Entities.TenantMerchantContractAggregate.Exceptions;
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
    public class UpdateWalletContractActivationStatusCommand : IRequest<int>
    {
        public int Id { get; set; }
        public bool Status { get; set; }
        public int? TenantId { get; set; }

        public UpdateWalletContractActivationStatusCommand(int id, bool status, int? tenantId = null)
        {
            Id = id;
            Status = status;
            TenantId = tenantId;
        }
    }

    public class UpdateWalletContractActivationStatusCommandHandler : IRequestHandler<UpdateWalletContractActivationStatusCommand, int>
    {
        private readonly IWalletContractRepository _walletContractRepository;
        private readonly IApplicationDbContextUnitOfWork _unitOfWork;
        private readonly IWalletContractReadOnlyRepository _walletContractReadOnlyRepository;

        public UpdateWalletContractActivationStatusCommandHandler(IWalletContractRepository walletContractRepository,
            IApplicationDbContextUnitOfWork unitOfWork,
            IWalletContractReadOnlyRepository walletContractReadOnlyRepository)
        {
            _walletContractRepository = walletContractRepository;
            _unitOfWork = unitOfWork;
            _walletContractReadOnlyRepository = walletContractReadOnlyRepository;
        }

        public async Task<int> Handle(UpdateWalletContractActivationStatusCommand request, CancellationToken cancellationToken)
        {
            var contract = await _walletContractRepository.GetAsync(request.Id, request.TenantId)
                           ?? throw new WalletContractNotFoundException("قرارداد پیدا نشد.");

            if (contract.GrantingProcessId.HasValue)
            {
                throw new WalletContractNotEditableException("فعال یا غیرفعال کردن این قرارداد ممکن نیست.");
            }

            var hasCashWallet = await _walletContractReadOnlyRepository.HasCashWalletContractByIdAsync(request.Id);
            if (hasCashWallet)
            {
                throw new WalletContractNotEditableException("فعال یا غیرفعال کردن این قرارداد ممکن نیست.");
            }

            if (contract.Status == WalletContractStatus.Reviewing || contract.Status == WalletContractStatus.Reject)
            {
                throw new WalletContractActivationStatusException("فقط قراردادهای تایید شده می توانند فعال یا غیر فعال شوند.");
            }

            if (request.Status)
            {
                if (contract.Status == WalletContractStatus.Active)
                {
                    throw new WalletContractActiveStatusException("قرارداد قبلا فعال شده است.");
                }

                var activeContract = await _walletContractReadOnlyRepository.GetActiveContractWithIdSmallerThan(contract.Id, contract.RootParentId, contract.TenantId);
                if (activeContract != null)
                {
                    activeContract.SetStatus(WalletContractStatus.DeActive);
                    activeContract.SetChangeStatusDate();

                    _walletContractRepository.Update(activeContract);

                }

                contract.SetStatus(WalletContractStatus.Active);
                contract.SetChangeStatusDate();
            }
            else
            {
                if (contract.Status == WalletContractStatus.DeActive)
                {
                    throw new WalletContractDeActiveStatusException("قرارداد قبلا غیرفعال شده است.");
                }

                contract.SetStatus(WalletContractStatus.DeActive);
                contract.SetChangeStatusDate();
            }

            contract.SetEditDateTime(DateTime.Now);

            _walletContractRepository.Update(contract);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return contract.Id;
        }

    }
}
