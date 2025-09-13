using Domain.Core.Entities;
using Domain.Core.Entities.WalletAggregate;
using Domain.Core.Entities.WalletAggregate.Exceptions;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Command.WalletCommands;

public class ActivateWalletCommand : IRequest
{
    public int WalletId { get; set; }
    public int TenantId { get; set; }

    public ActivateWalletCommand(int walletId, int tenantId)
    {
        WalletId = walletId;
        TenantId = tenantId;
    }
}

public class ActivateWalletCommandHandler : IRequestHandler<ActivateWalletCommand>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;

    public ActivateWalletCommandHandler(
        IWalletRepository walletRepository,
        IApplicationDbContextUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ActivateWalletCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByWalletIdAsync(request.WalletId) ?? throw new WalletNotFoundException("کیف پول یافت نشد"); ;
        if (wallet.TenantId != request.TenantId)
            throw new TenantForbiddenException("این کیف پول به این مالک زیرساخت تعلق ندارد");

        wallet.SetStatus(WalletStatus.Active);
        await ChangeDefaultWallet(request, wallet);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ChangeDefaultWallet(ActivateWalletCommand request, Wallet wallet)
    {
        var wallets = await _walletRepository.GetAsync(wallet.BusinessIdentityId, request.TenantId);
        bool hasDefaultWallet = wallets.Any(x => x.IsDefault);

        if (!hasDefaultWallet)
        {
            wallet.SetDefault(true);
        }
    }
}
