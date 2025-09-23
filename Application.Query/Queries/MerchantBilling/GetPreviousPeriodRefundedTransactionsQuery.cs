using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetPreviousPeriodRefundedTransactionsQuery(int TenantId, long Id) : IRequest<GetPreviousPeriodRefundedTransactionsViewModel>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetPreviousPeriodRefundedTransactionsQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetPreviousPeriodRefundedTransactionsQuery, GetPreviousPeriodRefundedTransactionsViewModel>
{
    public async Task<GetPreviousPeriodRefundedTransactionsViewModel> Handle(GetPreviousPeriodRefundedTransactionsQuery query, CancellationToken cancellationToken)
    {
        return await repository.GetPreviousPeriodRefundedPurchasesAsync(query);
    }
}