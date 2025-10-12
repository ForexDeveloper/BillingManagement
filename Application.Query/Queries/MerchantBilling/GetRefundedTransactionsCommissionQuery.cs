using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetRefundedTransactionsCommissionQuery(int TenantId, long Id) : IRequest<GetRefundedTransactionsCommissionViewModel>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetRefundedTransactionsCommissionQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetRefundedTransactionsCommissionQuery, GetRefundedTransactionsCommissionViewModel>
{
    public async Task<GetRefundedTransactionsCommissionViewModel> Handle(GetRefundedTransactionsCommissionQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.GetRefundedTransactionsCommissionAsync(query);
    }
}