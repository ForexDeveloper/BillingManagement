using MediatR;
using System.Threading;
using Domain.Core.Constants;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;
using Domain.Core.Entities.BillingAggregate.Exceptions;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetMerchantBillingQuery(int TenantId, long Id) : IRequest<GetMerchantBillingVm>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetMerchantBillingQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetMerchantBillingQuery, GetMerchantBillingVm>
{
    public async Task<GetMerchantBillingVm> Handle(GetMerchantBillingQuery query,
        CancellationToken cancellationToken)
    {
        var billing = await repository.GetBillingAsync(query);

        if (billing == null)
        {
            throw new BillingNotFoundException(BillingConstants.NotFoundMessage);
        }

        return billing;
    }
}