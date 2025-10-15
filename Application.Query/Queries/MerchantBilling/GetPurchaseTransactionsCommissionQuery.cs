using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetPurchaseTransactionsCommissionQuery(int TenantId, long Id) : IRequest<GetPurchaseTransactionsCommissionVm>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetCPurchaseTransactionsCommissionQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetPurchaseTransactionsCommissionQuery, GetPurchaseTransactionsCommissionVm>
{
    public async Task<GetPurchaseTransactionsCommissionVm> Handle(GetPurchaseTransactionsCommissionQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.GetPurchaseTransactionsCommissionAsync(query);
    }
}