using Application.Query.ReadOnlyRepositoryContracts;
using Application.Service.Contracts;
using Application.Service.Dtos.MerchantBillings;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Query.Queries.MerchantBilling;

public sealed record IsMerchantBillingPayableQuery(int TenantId, long Id, decimal Amount) : IRequest<bool>
{
    public long Id { get; set; } = Id;
    public int TenantId { get; set; } = TenantId;
    public decimal Amount { get; set; } = Amount;
}

public sealed class IsMerchantBillingPayableQueryHandler(IMerchantBillingReadOnlyRepository _merchantBillingReadOnlyRepository,
    IBillingPaymentService _billingPaymentService)
    : IRequestHandler<IsMerchantBillingPayableQuery, bool>
{
    public async Task<bool> Handle(IsMerchantBillingPayableQuery request, CancellationToken cancellationToken)
    {
        var billing = await _merchantBillingReadOnlyRepository.GetBillingByIdAsync(request.Id, request.TenantId);

        var merchantBillingPayableDto = new MerchantBillingPayableDto(
           request.TenantId, billing.Status, billing.DueDate, billing.GracePeriod, billing.PayableAmount, request.Amount);

        return _billingPaymentService.IsMerchantBillingPayable(merchantBillingPayableDto);
    }
}