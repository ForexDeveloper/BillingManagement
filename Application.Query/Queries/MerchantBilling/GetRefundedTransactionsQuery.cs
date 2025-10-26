using MediatR;
using System.Threading;
using Domain.Core.Constants;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;
using Domain.Core.Entities.BillingAggregate.Exceptions;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetRefundedTransactionsQuery(int TenantId, long Id) : IRequest<GetRefundedTransactionsVm>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetRefundedTransactionsQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetRefundedTransactionsQuery, GetRefundedTransactionsVm>
{
    public async Task<GetRefundedTransactionsVm> Handle(GetRefundedTransactionsQuery query, CancellationToken cancellationToken)
    {
        var transactions = await repository.GetRefundedTransactionsAsync(query);

        if (transactions == null)
        {
            throw new BillingNotFoundException(BillingConstants.NotFoundMessage);
        }

        return transactions;
    }
}