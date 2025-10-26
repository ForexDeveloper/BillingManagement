using MediatR;
using System.Threading;
using Domain.Core.Constants;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;
using Domain.Core.Entities.BillingAggregate.Exceptions;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetAdditionsQuery(int TenantId, long Id) : IRequest<GetAdditionsVm>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;
}

public sealed class GetAdditionsQueryHandler(IMerchantBillingReadOnlyRepository repository)
    : IRequestHandler<GetAdditionsQuery, GetAdditionsVm>
{
    public async Task<GetAdditionsVm> Handle(GetAdditionsQuery query, CancellationToken cancellationToken)
    {
        var additions = await repository.GetAdditionsAsync(query);

        if (additions == null)
        {
            throw new BillingNotFoundException(BillingConstants.NotFoundMessage);
        }

        return additions;
    }
}