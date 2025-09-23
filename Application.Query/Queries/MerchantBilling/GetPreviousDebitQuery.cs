using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetPreviousDebitQuery(int TenantId, long Id) : IRequest<GetPreviousDebitViewModel>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetMerchantBillingPreviousDebitQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetPreviousDebitQuery, GetPreviousDebitViewModel>
{
    public async Task<GetPreviousDebitViewModel> Handle(GetPreviousDebitQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.GetPreviousDebitAsync(query);
    }
}