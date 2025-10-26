using MediatR;
using System.Threading;
using Domain.Core.Constants;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;
using Domain.Core.Entities.BillingAggregate.Exceptions;

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
        var previousCredit = await repository.GetPreviousCreditAsync(query);

        if (previousCredit == null)
        {
            throw new BillingNotFoundException(BillingConstants.NotFoundMessage);
        }

        return previousCredit;
    }
}