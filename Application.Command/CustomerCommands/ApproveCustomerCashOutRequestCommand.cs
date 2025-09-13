using Application.Service.Contracts;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.WalletAggregate;
using Domain.Core.Entities.WalletAggregate.Exceptions;
using Domain.Core.Enums;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Command.CustomerCommands;

public class ApproveCustomerCashOutRequestCommand : IRequest<long>
{
    public long CashOutRequestId { get; set; }
    public int CustomerId { get; set; }
    public int TenantId { get; set; }
    public string Description { get; set; }
    public string BankTransactionCode { get; set; }

    public ApproveCustomerCashOutRequestCommand(int customerId, long cashOutRequestId, string bankTransactionCode, string description, int tenantId)
    {
        CustomerId = customerId;
        CashOutRequestId = cashOutRequestId;
        Description = description;
        TenantId = tenantId;
        BankTransactionCode = bankTransactionCode;
    }
}

public class ApproveCustomerCashOutCommandHandler : IRequestHandler<ApproveCustomerCashOutRequestCommand, long>
{
    private readonly ICashOutRequestService _cashOutRequestService;
    private readonly IWalletRepository _walletRepository;

    public ApproveCustomerCashOutCommandHandler(ICashOutRequestService cashOutRequestService, IWalletRepository walletRepository)
    {
        _cashOutRequestService = cashOutRequestService;
        _walletRepository = walletRepository;
    }

    public async Task<long> Handle(ApproveCustomerCashOutRequestCommand request, CancellationToken cancellationToken)
    {
        var cashOutRequest = await _cashOutRequestService.GetCashOutRequest(request.CashOutRequestId, request.CustomerId, request.TenantId);

        var customer = await _cashOutRequestService.ValidateCustomer(request.CustomerId, cashOutRequest.TenantId);

        var cashWallet = await _walletRepository.GetCashWalletAsync(request.CustomerId) ?? throw new WalletNotFoundException("کیف پول یافت نشد");
        if (cashWallet.Status == WalletStatus.Deactive)
        {
            throw new ArgumentValidationException(nameof(cashWallet.Status), "کیف پول غیرفعال می باشد");
        }
        if (cashWallet.Status == WalletStatus.Suspend)
        {
            throw new ArgumentValidationException(nameof(cashWallet.Status), "کیف پول معلق می باشد");
        }

        await _cashOutRequestService.AproveCustomerCashOutRequest(cashOutRequest, request.BankTransactionCode, request.Description, customer, cancellationToken);

        return request.CashOutRequestId;
    }
}