using System;
using Domain.Core.Enums;
using Shared.EventBus.Events;
using System.Threading.Tasks;
using Shared.EventBus.Contracts;
using Application.Service.Contracts;
using Domain.Core.UnitOfWorkContracts;
using Domain.Core.Entities.Shared.Exceptions;
using Application.Service.Dtos.MerchantBillings;
using Domain.Core.Entities.MerchantBillingAggregate;
using Domain.Core.Entities.BillingAggregate.Exceptions;

namespace Application.Service.Services;

public sealed class BillingPaymentService(
    IOutboxService outboxService,
    IApplicationDbContextUnitOfWork unitOfWork,
    IMerchantBillingRepository merchantBillingRepository) : IBillingPaymentService
{
    public bool IsMerchantBillingPayable(MerchantBillingPayableDto request)
    {
        if (request.PayAmount > request.PayableAmount)
        {
            throw new ArgumentValidationException("BillingId", "مبلغ پرداختی بیشتر از مبلغ قابل پرداخت صورت حساب می باشد.");
        }

        if (request.Status == BillingStatus.Settled)
        {
            throw new ArgumentValidationException("BillingId", "صورت حساب قبلا پرداخت شده است.");
        }

        if (request.Status == BillingStatus.Overdue)
        {
            throw new ArgumentValidationException("BillingId", "صورت حساب معوق قابل پرداخت نمی باشد.");
        }

        if (request.DueDate.AddDays(request.GracePeriod).Date < DateTime.Today)
        {
            throw new ArgumentValidationException("BillingId", "صورت حساب قابل پرداخت نمی باشد.");
        }

        return true;
    }

    public async Task SetMerchantBillingPayment(PmBillingManualPaymentUpdateStateEvent request)
    {
        var billing = await merchantBillingRepository.GetAsync(request.BillingId) ?? throw new BillingNotFoundException("صورت حساب پیدا نشد.");

        var payableAmount = billing.GetPayableAmount();

        var merchantBillingPayableDto = new MerchantBillingPayableDto(
            billing.TenantId, billing.Status, billing.DueDate, billing.GracePeriod, payableAmount, request.Amount);

        IsMerchantBillingPayable(merchantBillingPayableDto);

        billing.AddBillingPayment(request.BillingId, request.PaymentId, request.Amount, request.PaymentDate);

        if (request.Amount == payableAmount)
        {
            billing.Settle();
        }
        else
        {
            billing.PartialPay();
        }

        outboxService.AddNewEvent(new BmBillingManualPaymentSettledEvent
        {
            BillingId = request.BillingId,
            PaymentId = request.PaymentId,
            Amount = request.Amount,
        });

        merchantBillingRepository.Update(billing);

        await unitOfWork.SaveChangesAsync();
    }
}
