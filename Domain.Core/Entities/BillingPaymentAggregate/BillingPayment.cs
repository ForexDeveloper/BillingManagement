using Domain.Base;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Helper;
using System;

namespace Domain.Core.Entities.BillingPaymentAggregate;

public sealed class BillingPayment : BaseEntity<long>
{
    public long BillingId { get; protected set; }

    public long PaymentId { get; protected set; }

    public decimal Amount { get; protected set; }

    public DateTime PaymentDate { get; protected set; }

    public string CheckSum { get; protected set; }

    public Billing Billing { get; protected set; }

    protected BillingPayment()
    {

    }

    public BillingPayment(long billingId, long paymentId, decimal amount, DateTime paymentDate)
    {
        BillingId = billingId;
        PaymentId = paymentId;
        SetAmount(amount);
        PaymentDate = paymentDate;
        SetCheckSum();
    }

    private void SetAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentValidationException(nameof(amount), "مبلغ پرداختی صورتحساب نمی تواند کوچک تر مساوی صفر باشد");

        Amount = amount;
    }

    private void SetCheckSum()
    {
        CheckSum = $"{BillingId}{PaymentId}{Amount:F10}/{PaymentDate:yyyy-MM-ddTHH:mm:ss}".Hash();
    }

    protected void ValidateCheckSum()
    {
        var comperedTo = $"{BillingId}{PaymentId}{Amount:F10}{PaymentDate:yyyy-MM-ddTHH:mm:ss}";

        if (!comperedTo.Validate(CheckSum))
        {
            throw new ArgumentValidationException("InconsistentData", "اطلاعات موجود در دیتابیس صحیح نمی باشد.");
        }
    }
}