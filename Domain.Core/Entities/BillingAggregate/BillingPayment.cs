using Domain.Base;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TransactionAggregate;
using Domain.Core.Enums;
using Domain.Core.Helper;
using System;

namespace Domain.Core.Entities.BillingAggregate;

public class BillingPayment : BaseEntity<long>
{
    #region Property

    public long BillingId { get; private set; }
    public Billing Billing { get; private set; }
    public DateTime PayDate { get; private set; }
    public decimal Amount { get; private set; }
    public string CheckSum { get; private set; }
    public long? TransactionId { get; private set; }
    public Transaction Transaction { get; private set; }
    public BillingPaymentState State { get; private set; }
    public long? PaymentDetailId { get; private set; }

    #endregion

    private BillingPayment()
    {

    }

    public BillingPayment(long billingId, DateTime payDate, decimal amount, Transaction transaction, BillingPaymentState state,
        long? paymentDetailId = null)
    {
        BillingId = billingId;
        PayDate = payDate;
        Transaction = transaction;
        State = state;
        PaymentDetailId = paymentDetailId;
        SetAmount(amount);
        SetCheckSum();
    }

    public void SetAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentValidationException(nameof(amount), "مبلغ پرداختی صورتحساب نمی تواند کوچک تر مساوی صفر باشد");

        Amount = amount;
    }

    private void SetCheckSum()
    {
        CheckSum = $"{BillingId}{Amount:F10}{State}{PaymentDetailId}{PayDate:yyyy-MM-ddTHH:mm:ss}".Hash();
    }
}
