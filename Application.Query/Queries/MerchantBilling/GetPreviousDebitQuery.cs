using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetPreviousDebitQuery(int TenantId, long Id) : IRequest<GetPreviousDebitVm>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetDebitQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetPreviousDebitQuery, GetPreviousDebitVm>
{
    public async Task<GetPreviousDebitVm> Handle(GetPreviousDebitQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.GetPreviousDebitAsync(query);
    }
}