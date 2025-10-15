using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetMerchantBillingQuery(int TenantId, long Id) : IRequest<GetMerchantBillingVm>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetMerchantBillingQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetMerchantBillingQuery, GetMerchantBillingVm>
{
    public async Task<GetMerchantBillingVm> Handle(GetMerchantBillingQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.GetBillingAsync(query);
    }
}