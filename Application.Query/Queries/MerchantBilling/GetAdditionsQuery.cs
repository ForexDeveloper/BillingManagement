using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetAdditionsQuery(int TenantId, long Id) : IRequest<GetAdditionsViewModel>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetAdditionsQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetAdditionsQuery, GetAdditionsViewModel>
{
    public async Task<GetAdditionsViewModel> Handle(GetAdditionsQuery query, CancellationToken cancellationToken)
    {
        return await repository.GetAdditionsAsync(query);
    }
}