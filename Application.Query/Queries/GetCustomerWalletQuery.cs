using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Wallets;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.WalletAggregate.Exceptions;
using Domain.Core.Enums;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries;

public class GetCustomerWalletQuery : IRequest<GetCustomerWalletViewModel>
{
    public int Id { get; }
    public int WalletId { get; set; }
    public GetCustomerWalletQuery(int id, int walletId)
    {
        Id = id;
        WalletId = walletId;
    }
}

public class CustomerWalletQueryHandler : IRequestHandler<GetCustomerWalletQuery, GetCustomerWalletViewModel>
{
    private readonly IWalletReadOnlyRepository _walletReadOnlyRepository;
    private readonly IAttachmentReadOnlyRepository _attachmentRepositoryReadOnlyRepository;
    public readonly IBillingReadOnlyRepository _billingRepository;
    public CustomerWalletQueryHandler(IWalletReadOnlyRepository walletReadOnlyRepository, IAttachmentReadOnlyRepository attachmentRepositoryReadOnlyRepository, IBillingReadOnlyRepository billingRepository)
    {
        _walletReadOnlyRepository = walletReadOnlyRepository;
        _attachmentRepositoryReadOnlyRepository = attachmentRepositoryReadOnlyRepository;
        _billingRepository = billingRepository;
    }

    public async Task<GetCustomerWalletViewModel> Handle(GetCustomerWalletQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _walletReadOnlyRepository.GetWalletAsync(request.Id, request.WalletId);

        if (wallet is null)
            throw new WalletNotFoundException("کیف پول یافت نشد.");

        var currentBilling = await _billingRepository.GetLastUnpaidBillOfCurrentMonth(wallet.AccountId, cancellationToken);

        var attachment = await _attachmentRepositoryReadOnlyRepository.GetAsync(EntityType.Plan, wallet.PlanId);

        return new GetCustomerWalletViewModel
        {
            Title = wallet.Title,
            Type = wallet.Type,
            Balance = wallet.Balance,
            BillStatus = currentBilling.BillStatus,
            LastBillId = currentBilling.LastBillId,
            LastBillDate = currentBilling.LastBillDate,
            LastBillAmount = currentBilling.LastBillAmount,
            Status = wallet.Status,
            IsDefault = wallet.IsDefault,
            LogoId = attachment?.FileReference,
            TermsAndConditions = wallet.TermsAndConditions
        };
    }
}
