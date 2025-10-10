using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetRefundedTransactionsQuery(int TenantId, long Id) : IRequest<GetRefundedTransactionsViewModel>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetRefundedTransactionsQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetRefundedTransactionsQuery, GetRefundedTransactionsViewModel>
{
    public async Task<GetRefundedTransactionsViewModel> Handle(GetRefundedTransactionsQuery query, CancellationToken cancellationToken)
    {
        return await repository.GetPreviousPeriodRefundedPurchasesAsync(query);
    }
}