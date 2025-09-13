using Application.Query.Base;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.Wallets;
using Domain.Core.Entities.WalletAggregate.Exceptions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries;

public class GetCustomerCashWalletQuery(int customerId, int? tenantId) : IRequest<CashWalletViewModel>
{
    public int CustomerId { get; set; } = customerId;
    public int? TenantId { get; set; } = tenantId;

}

public class GetCustomerCashWalletQueryHandler : BaseQueryHandler, IRequestHandler<GetCustomerCashWalletQuery, CashWalletViewModel>
{
    private readonly IWalletReadOnlyRepository _walletReadOnlyRepository;
    private readonly ICustomerReadOnlyRepository _customerReadOnlyRepository;

    public GetCustomerCashWalletQueryHandler(IWalletReadOnlyRepository walletReadOnlyRepository, ICustomerReadOnlyRepository customerReadOnlyRepository)
    {
        _walletReadOnlyRepository = walletReadOnlyRepository;
        _customerReadOnlyRepository = customerReadOnlyRepository;
    }

    public async Task<CashWalletViewModel> Handle(GetCustomerCashWalletQuery request, CancellationToken cancellationToken)
    {
        var cashWallet = await _walletReadOnlyRepository.GetCashWalletByCustomerIdAsync(request.CustomerId, request.TenantId);

        if (cashWallet == null)
            return null;

        return new CashWalletViewModel
        {
            Id = cashWallet.Id,
            Balance = cashWallet.Balance,
            NonWithDrawableBalance = cashWallet.NonWithDrawableBalance,
            WithDrawableBalance = cashWallet.WithDrawableBalance,
            Status = cashWallet.Status,
            TermsAndConditions = cashWallet.TermsAndConditions
        };
    }
}