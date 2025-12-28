using System;
using Domain.Base;
using Domain.Core.Helper;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.Shared.Exceptions;

namespace Domain.Core.Entities.BillingPaymentAggregate;

public sealed class BillingPayment : BaseEntity<long>
{
    public long BillingId { get; private set; }

    public long PaymentId { get; private set; }

    public decimal Amount { get; private set; }

    public DateTime PaymentDate { get; private set; }

    public string CheckSum { get; private set; }

    public Billing Billing { get; private set; }

    private BillingPayment()
    {

    }

    public BillingPayment(long paymentId, decimal amount, DateTime paymentDate)
    {
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