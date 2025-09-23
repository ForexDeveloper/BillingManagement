using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetDeductionsQuery(long Id, int TenantId) : IRequest<GetDeductionsViewModel>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetDeductionsQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetDeductionsQuery, GetDeductionsViewModel>
{
    public async Task<GetDeductionsViewModel> Handle(GetDeductionsQuery query, CancellationToken cancellationToken)
    {
        return await repository.GetDeductionsAsync(query);
    }
}