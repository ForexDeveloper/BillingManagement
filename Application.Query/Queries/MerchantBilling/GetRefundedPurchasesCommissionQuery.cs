using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetRefundedPurchasesCommissionQuery(long Id, int TenantId) : IRequest<GetRefundedTransactionsCommissionViewModel>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetRefundedPurchasesCommissionQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetRefundedPurchasesCommissionQuery, GetRefundedTransactionsCommissionViewModel>
{
    public async Task<GetRefundedTransactionsCommissionViewModel> Handle(GetRefundedPurchasesCommissionQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.GetRefundedPurchasesCommissionAsync(query);
    }
}