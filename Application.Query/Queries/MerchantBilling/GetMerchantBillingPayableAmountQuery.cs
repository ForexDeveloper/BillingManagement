using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Application.Service.Contracts;
using Application.Service.Dtos.MerchantBillings;
using Application.Query.ReadOnlyRepositoryContracts;
using Domain.Core.Entities.BillingAggregate.Exceptions;

namespace Application.Query.Queries.MerchantBilling;

public sealed record GetMerchantBillingPayableAmountQuery(int TenantId, long Id, decimal Amount) : IRequest<decimal>
{
    public long Id { get; set; } = Id;

    public int TenantId { get; set; } = TenantId;

    public decimal Amount { get; set; } = Amount;
}

public sealed class IsMerchantBillingPayableQueryHandler(
    IMerchantBillingReadOnlyRepository _merchantBillingReadOnlyRepository,
    IBillingPaymentService _billingPaymentService) : IRequestHandler<GetMerchantBillingPayableAmountQuery, decimal>
{
    public async Task<decimal> Handle(GetMerchantBillingPayableAmountQuery request, CancellationToken cancellationToken)
    {
        var billing = await _merchantBillingReadOnlyRepository.GetBillingByIdAsync(request.Id, request.TenantId) ??
                      throw new BillingNotFoundException("صورت حساب پیدا نشد.");

        var merchantBillingPayableDto = new MerchantBillingPayableDto(request.TenantId, billing.Status, billing.DueDate,
            billing.GracePeriod, billing.PayableAmount, request.Amount);

        var isMerchantBillingPayable = _billingPaymentService.IsMerchantBillingPayable(merchantBillingPayableDto);

        return isMerchantBillingPayable ? billing.PayableAmount : 0;
    }
}