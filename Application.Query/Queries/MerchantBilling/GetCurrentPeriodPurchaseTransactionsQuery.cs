using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.ViewModels.MerchantBillings;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetCurrentPeriodPurchaseTransactionsQuery : IRequest<GetCurrentPeriodPurchaseTransactionsViewModel>
{
}

public sealed class GetCurrentPeriodPurchaseTransactionsQueryHandler : IRequestHandler<GetCurrentPeriodPurchaseTransactionsQuery, GetCurrentPeriodPurchaseTransactionsViewModel>
{
    public async Task<GetCurrentPeriodPurchaseTransactionsViewModel> Handle(GetCurrentPeriodPurchaseTransactionsQuery query, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}