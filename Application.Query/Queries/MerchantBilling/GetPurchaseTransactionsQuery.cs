using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.ViewModels.MerchantBillings;
using Application.Query.ReadOnlyRepositoryContracts;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetPurchaseTransactionsQuery(int TenantId, long Id) : IRequest<GetPurchaseTransactionsViewModel>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetPurchaseTransactionsQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetPurchaseTransactionsQuery, GetPurchaseTransactionsViewModel>
{
    public async Task<GetPurchaseTransactionsViewModel> Handle(GetPurchaseTransactionsQuery query, CancellationToken cancellationToken)
    {
        return await repository.GetPurchaseTransactionsAsync(query);
    }
}