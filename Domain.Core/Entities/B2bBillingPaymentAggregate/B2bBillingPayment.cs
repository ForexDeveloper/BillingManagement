using System;
using Domain.Base;
using Domain.Core.Helper;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.B2bBillingAggregate;

namespace Domain.Core.Entities.B2bBillingPaymentAggregate;

public abstract class B2bBillingPayment : BaseEntity<long>
{
    public long BillingId { get; protected set; }

    public decimal Amount { get; protected set; }

    public DateTime PaymentDate { get; protected set; }

    public string CheckSum { get; protected set; }

    public B2bBilling Billing { get; protected set; }

    protected B2bBillingPayment()
    {

    }

    private void SetAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentValidationException(nameof(amount), "مبلغ پرداختی صورتحساب نمی تواند کوچک تر مساوی صفر باشد");

        Amount = amount;
    }

    private void SetCheckSum()
    {
        //CheckSum = $"{BillingId}{Amount:F10}/*{State}{PaymentDetailId}*/{PaymentDate:yyyy-MM-ddTHH:mm:ss}".Hash();

        CheckSum = $"{BillingId}{Amount:F10}/{PaymentDate:yyyy-MM-ddTHH:mm:ss}".Hash();
    }
}