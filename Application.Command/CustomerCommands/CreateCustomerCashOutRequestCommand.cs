using Application.Service.Contracts;
using Application.Service.Dtos.CashWallet;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.WalletAggregate;
using Domain.Core.Entities.WalletAggregate.Exceptions;
using Domain.Core.Enums;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Command.CustomerCommands;

public class CreateCustomerCashOutRequestCommand : IRequest<long>
{
    public int BankAccountId { get; set; }
    public decimal Amount { get; set; }
    public int CustomerId { get; set; }
    public int TenantId { get; set; }

    public CreateCustomerCashOutRequestCommand(int bankAccountId, decimal amount, int customerId, int tenantId)
    {
        BankAccountId = bankAccountId;
        Amount = amount;
        CustomerId = customerId;
        TenantId = tenantId;
    }
}

public class CreateCashOutRequestCommandHandler : IRequestHandler<CreateCustomerCashOutRequestCommand, long>
{
    private readonly ICashOutRequestService _cashOutRequestService;
    private readonly IWalletRepository _walletRepository;

    public CreateCashOutRequestCommandHandler(ICashOutRequestService cashOutRequestService,
        IWalletRepository walletRepository)
    {
        _cashOutRequestService = cashOutRequestService;
        _walletRepository = walletRepository;
    }

    public async Task<long> Handle(CreateCustomerCashOutRequestCommand request, CancellationToken cancellationToken)
    {
        var customer = await _cashOutRequestService.ValidateCustomer(request.CustomerId, request.TenantId);

        var cashWallet = await _walletRepository.GetCashWalletAsync(request.CustomerId) ?? throw new WalletNotFoundException("کیف پول یافت نشد");
        if (cashWallet.Status == WalletStatus.Deactive)
        {
            throw new ArgumentValidationException(nameof(cashWallet.Status), "کیف پول غیرفعال می باشد");
        }
        if (cashWallet.Status == WalletStatus.Suspend)
        {
            throw new ArgumentValidationException(nameof(cashWallet.Status), "کیف پول معلق می باشد");
        }

        await _cashOutRequestService.ValidateBankAccount(request.BankAccountId, customer);

        var cashOutRequest = await _cashOutRequestService.CreateCashOutRequest(new CreateCashOutRequestDto()
        {
            BusinessIdentityId = request.CustomerId,
            TenantId = request.TenantId,
            Amount = request.Amount,
            BankAccountId = request.BankAccountId
        }, cancellationToken);

        return cashOutRequest.Id;
    }
}