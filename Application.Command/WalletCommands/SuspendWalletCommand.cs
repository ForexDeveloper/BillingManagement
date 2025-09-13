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

public class SuspendWalletCommand : IRequest
{
    public int WalletId { get; set; }
    public int TenantId { get; set; }

    public SuspendWalletCommand(int walletId, int tenantId)
    {
        WalletId = walletId;
        TenantId = tenantId;
    }
}

public class SuspendWalletCommandHandler : IRequestHandler<SuspendWalletCommand>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;

    public SuspendWalletCommandHandler(
        IWalletRepository walletRepository,
        IApplicationDbContextUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(SuspendWalletCommand request, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository.GetByWalletIdAsync(request.WalletId) ?? throw new WalletNotFoundException("کیف پول یافت نشد");
        if (wallet.TenantId != request.TenantId)
            throw new TenantForbiddenException("این کیف پول به این مالک زیرساخت تعلق ندارد");

        wallet.SetStatus(WalletStatus.Suspend);

        if (wallet is LoanWallet)
        {
            await ChangeDefaultWallet(request, wallet);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ChangeDefaultWallet(SuspendWalletCommand request, Wallet wallet)
    {
        var wallets = await _walletRepository.GetAsync(wallet.BusinessIdentityId, request.TenantId);
        foreach (var item in wallets)
        {
            item.SetDefault(false);
        }

        var defaultWallet = wallets.OrderByDescending(x => x.Account.Balance).FirstOrDefault(x => x.Id != request.WalletId);
        defaultWallet?.SetDefault(true);
    }
}
