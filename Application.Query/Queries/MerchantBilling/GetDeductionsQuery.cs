using MediatR;
using System.Threading;
using Domain.Core.Constants;
using System.Threading.Tasks;
using Application.Query.ReadOnlyRepositoryContracts;
using Application.Query.ViewModels.MerchantBillings;
using Domain.Core.Entities.BillingAggregate.Exceptions;

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
        var deductions = await repository.GetDeductionsAsync(query);

        if (deductions == null)
        {
            throw new BillingNotFoundException(BillingConstants.NotFoundMessage);
        }

        return deductions;
    }
}