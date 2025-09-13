using Domain.Base;
using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TransactionAggregate;
using Domain.Core.Entities.WalletAggregate;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Enums;
using Domain.Core.Helper;

namespace Domain.Core.Entities.FinancialDocumentAggregate;

public class FinancialDocumentPayment : BaseEntity<long>
{
    // اعتباری + مازاد نقدی تقسیط به مرچنت
    #region Property
    public int FromBusinessIdentityId { get; private set; }
    public BusinessIdentity FromBusinessIdentity { get; private set; }

    public int ToBusinessIdentityId { get; private set; }
    public BusinessIdentity ToBusinessIdentity { get; private set; }

    public FinancialDocumentPaymentType Type { get; private set; }
    public decimal Amount { get; private set; }

    public int? WalletId { get; private set; }
    public Wallet Wallet { get; private set; }

    public int? WalletContractId { get; private set; }
    public WalletContract WalletContract { get; private set; }

    public long FinancialDocumentId { get; private set; }
    public FinancialDocument FinancialDocument { get; private set; }

    public long TransactionId { get; private set; }
    public Transaction Transaction { get; private set; }

    public long? PaymentDetailId { get; private set; }
    public string CheckSum { get; private set; }

    #endregion

    private FinancialDocumentPayment()
    {

    }

    public FinancialDocumentPayment(int fromBusinessIdentityId, int toBusinessIdentityId, FinancialDocumentPaymentType type, decimal amount,
        int? walletId, int? walletContractId, long transactionId, long? paymentDetailId)
    {
        FromBusinessIdentityId = fromBusinessIdentityId;
        ToBusinessIdentityId = toBusinessIdentityId;
        Type = type;
        Amount = amount;
        WalletId = walletId;
        WalletContractId = walletContractId;
        TransactionId = transactionId;
        PaymentDetailId = paymentDetailId;
        SetCheckSum();
    }

    public FinancialDocumentPayment(int fromBusinessIdentityId, int toBusinessIdentityId, FinancialDocumentPaymentType type, decimal amount,
        int? walletId, int? walletContractId, Transaction transaction, long? paymentDetailId)
    {
        FromBusinessIdentityId = fromBusinessIdentityId;
        ToBusinessIdentityId = toBusinessIdentityId;
        Type = type;
        WalletId = walletId;
        WalletContractId = walletContractId;
        Transaction = transaction;
        PaymentDetailId = paymentDetailId;
        SetAmount(amount);
        SetCheckSum();
    }

    public void SetAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentValidationException(nameof(amount), "مبلغ پرداختی سند مالی نمی تواند کوچک تر مساوی صفر باشد");

        Amount = amount;
    }

    public void SetCheckSum()
    {
        CheckSum = HashHelper.Hash($"{FromBusinessIdentityId}{ToBusinessIdentityId}{Type}{Amount:F10}{CreatedDateTime:yyyy-MM-ddTHH:mm:ss}");
    }

    public void SetTransaction(Transaction transaction)
    {
        Transaction = transaction;
    }

    public void ValidateCheckSum()
    {
        string comperedTo = $"{FromBusinessIdentityId}{ToBusinessIdentityId}{Type}{Amount:F10}{CreatedDateTime:yyyy-MM-ddTHH:mm:ss}";

        if (!comperedTo.Validate(CheckSum))
            throw new ArgumentValidationException("InconsistentData", "اطلاعات موجود در دیتابیس صحیح نمی باشد.");
    }
}
