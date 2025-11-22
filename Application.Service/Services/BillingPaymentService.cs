using Application.Service.Contracts;
using Application.Service.Dtos.MerchantBillings;
using Domain.Core.Entities.BillingAggregate.Exceptions;
using Domain.Core.Entities.MerchantBillingAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Enums;
using Domain.Core.UnitOfWorkContracts;
using Shared.EventBus.Contracts;
using Shared.EventBus.Events;
using System;
using System.Threading.Tasks;

namespace Application.Service.Services;
public class BillingPaymentService(IMerchantBillingRepository merchantBillingRepository, IOutboxService outboxService, IApplicationDbContextUnitOfWork unitOfWork) : IBillingPaymentService
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

    public async Task MerchantBillingPayment(PmBillingManualPaymentUpdateStateEvent requset)
    {
        var billing = await merchantBillingRepository.GetAsync(requset.BillingId) ?? throw new BillingNotFoundException("صورت حساب پیدا نشد.");
        var payableAmount = billing.GetPayableAmount();

        var merchantBillingPayableDto = new MerchantBillingPayableDto(
            billing.TenantId, billing.Status, billing.DueDate, billing.GracePeriod, payableAmount, requset.Amount);


        IsMerchantBillingPayable(merchantBillingPayableDto);

        billing.AddBillingPayment(requset.BillingId, requset.PaymentId, requset.Amount, requset.PaymentDate);

        if (requset.Amount == payableAmount)
        {
            billing.Settle();
        }
        else
        {
            billing.PartialPay();
        }

        outboxService.AddNewEvent(new BmBillingManualPaymentSettledEvent
        {
            BillingId = requset.BillingId,
            PaymentId = requset.PaymentId,
            Amount = requset.Amount,
        });

        merchantBillingRepository.Update(billing);

        await unitOfWork.SaveChangesAsync();
    }
}
