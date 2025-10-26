using MediatR;
using System.Threading;
using Domain.Core.Constants;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;
using Domain.Core.Entities.BillingAggregate.Exceptions;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetRefundedTransactionsCommissionQuery(int TenantId, long Id) : IRequest<GetRefundedTransactionsCommissionVm>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetRefundedTransactionsCommissionQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetRefundedTransactionsCommissionQuery, GetRefundedTransactionsCommissionVm>
{
    public async Task<GetRefundedTransactionsCommissionVm> Handle(GetRefundedTransactionsCommissionQuery query,
        CancellationToken cancellationToken)
    {
        var commission = await repository.GetRefundedTransactionsCommissionAsync(query);

        if (commission == null)
        {
            throw new BillingNotFoundException(BillingConstants.NotFoundMessage);
        }

        return commission;
    }
}