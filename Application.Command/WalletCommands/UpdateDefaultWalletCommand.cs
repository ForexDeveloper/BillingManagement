using Domain.Core.Entities.WalletAggregate;
using Domain.Core.Entities.WalletAggregate.Exceptions;
using Domain.Core.UnitOfWorkContracts;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Command.WalletCommands;

public class UpdateDefaultWalletCommand : IRequest
{
    public int WalletId { get; set; }
    public int CustomerId { get; set; }

    public UpdateDefaultWalletCommand(int walletId, int customerId)
    {
        WalletId = walletId;
        CustomerId = customerId;
    }
}

public class UpdateDefaultWalletCommandHandler : IRequestHandler<UpdateDefaultWalletCommand>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IApplicationDbContextUnitOfWork _unitOfWork;

    public UpdateDefaultWalletCommandHandler(
        IWalletRepository walletRepository,
        IApplicationDbContextUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateDefaultWalletCommand request, CancellationToken cancellationToken)
    {
        var wallets = await _walletRepository.GetAsync(request.CustomerId);
        if (!wallets.Any(x => x.Id == request.WalletId))
        {
            throw new WalletNotFoundException("کیف پول یافت نشد.");
        }

        foreach (var wallet in wallets)
        {
            wallet.SetDefault(false);

            if (wallet.Id == request.WalletId)
            {
                wallet.SetDefault(true);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
