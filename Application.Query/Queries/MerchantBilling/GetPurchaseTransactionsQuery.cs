using MediatR;
using System.Threading;
using Domain.Core.Constants;
using System.Threading.Tasks;
using Application.Query.ViewModels.MerchantBillings;
using Application.Query.ReadOnlyRepositoryContracts;
using Domain.Core.Entities.BillingAggregate.Exceptions;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetPurchaseTransactionsQuery(int TenantId, long Id) : IRequest<GetPurchaseTransactionsVm>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetPurchaseTransactionsQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetPurchaseTransactionsQuery, GetPurchaseTransactionsVm>
{
    public async Task<GetPurchaseTransactionsVm> Handle(GetPurchaseTransactionsQuery query, CancellationToken cancellationToken)
    {
        var transactions = await repository.GetPurchaseTransactionsAsync(query);

        if (transactions == null)
        {
            throw new BillingNotFoundException(BillingConstants.NotFoundMessage);
        }

        return transactions;
    }
}