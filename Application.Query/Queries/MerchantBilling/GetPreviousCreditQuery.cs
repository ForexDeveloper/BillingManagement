using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetPreviousCreditQuery(int TenantId, long Id) : IRequest<GetPreviousCreditVm>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetPreviousCreditQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetPreviousCreditQuery, GetPreviousCreditVm>
{
    public async Task<GetPreviousCreditVm> Handle(GetPreviousCreditQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.GetPreviousCreditAsync(query);
    }
}