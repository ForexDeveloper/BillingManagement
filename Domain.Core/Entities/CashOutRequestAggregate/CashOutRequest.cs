using Domain.Base;
using Domain.Core.Entities.BankAccountAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.WalletAggregate;
using Domain.Core.Enums;

namespace Domain.Core.Entities.CashOutRequestAggregate;

public class CashOutRequest : BaseEntity<long>
{
    #region Property
    public int CashWalletId { get; private set; }
    public CashWallet CashWallet { get; private set; }

    public long? FinancialDocumentId { get; private set; }
    public FinancialDocument FinancialDocument { get; private set; }

    public int BankAccountId { get; private set; }
    public BankAccount BankAccount { get; private set; }

    public CashOutRequestStatus Status { get; private set; }

    public CashOutRequestRejectReason? RejectReason { get; private set; }

    public string Description { get; private set; }

    public string BankTransactionCode { get; private set; }

    public long FollowUpCode { get; private set; }

    public int TenantId { get; private set; }
    public Tenant Tenant { get; private set; }

    #endregion

    private CashOutRequest() { }

    public CashOutRequest(int cashWalletId, int financialDocumentId, int bankAccountId)
    {
        CashWalletId = cashWalletId;
        FinancialDocumentId = financialDocumentId;
        BankAccountId = bankAccountId;
        Status = CashOutRequestStatus.AwaitingApproval;
    }

    public CashOutRequest(int cashWalletId, FinancialDocument financialDocument, int bankAccountId, int tenantId)
    {
        CashWalletId = cashWalletId;
        FinancialDocument = financialDocument;
        BankAccountId = bankAccountId;
        TenantId = tenantId;
        Status = CashOutRequestStatus.AwaitingApproval;
    }

    public void Approve(string bankTransactionCode, string description)
    {
        if (string.IsNullOrEmpty(bankTransactionCode))
            throw new ArgumentValidationException(nameof(bankTransactionCode), "شناسه تراکنش بانکی نامعتبر می باشد");

        if (bankTransactionCode.Length > 20)
            throw new ArgumentValidationException(nameof(bankTransactionCode), "طول کارکتر های شناسه تراکنش بانکی غیر مجاز می باشد می باشد.");

        if (Status != CashOutRequestStatus.AwaitingApproval)
            throw new ArgumentValidationException(nameof(Status), "فقط درخواست های در وضعیت در حال بررسی قابلیت تغییر وضعیت به پرداخت شده را دارند");

        Status = CashOutRequestStatus.Paid;
        BankTransactionCode = bankTransactionCode;
        Description = description;
    }

    public void Reject(CashOutRequestRejectReason reason, string description)
    {
        if (Status != CashOutRequestStatus.AwaitingApproval)
            throw new ArgumentValidationException(nameof(Status), "فقط درخواست های در وضعیت در حال بررسی قابلیت تغییر وضعیت به رد شده را دارند");

        Status = CashOutRequestStatus.Rejected;

        RejectReason = reason;

        Description = description;
    }
}