using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetDeductionsQuery(int TenantId, long Id) : IRequest<GetDeductionsVm>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetDeductionsQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetDeductionsQuery, GetDeductionsVm>
{
    public async Task<GetDeductionsVm> Handle(GetDeductionsQuery query, CancellationToken cancellationToken)
    {
        return await repository.GetDeductionsAsync(query);
    }
}