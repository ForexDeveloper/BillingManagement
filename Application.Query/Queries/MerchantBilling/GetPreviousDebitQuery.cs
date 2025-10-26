using MediatR;
using System.Threading;
using Domain.Core.Constants;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;
using Domain.Core.Entities.BillingAggregate.Exceptions;

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
        var previousDebit = await repository.GetPreviousDebitAsync(query);

        if (previousDebit == null)
        {
            throw new BillingNotFoundException(BillingConstants.NotFoundMessage);
        }

        return previousDebit;
    }
}