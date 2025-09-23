using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetMerchantBillingQuery(int TenantId, long Id) : IRequest<GetMerchantBillingViewModel>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetMerchantBillingQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetMerchantBillingQuery, GetMerchantBillingViewModel>
{
    public async Task<GetMerchantBillingViewModel> Handle(GetMerchantBillingQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.GetBillingAsync(query);
    }
}