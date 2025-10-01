using System;
using Domain.Base;
using Domain.Core.Enums;
using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.FinancialDocumentAggregate;

namespace Domain.Core.Entities.InstallmentAggregate;

public abstract class Installment : BaseEntity<long>
{
    public int TenantId { get; protected set; }

    public long FinancialDocumentId { get; protected set; }

    public long? BillingId { get; protected set; }

    public int FromBusinessIdentityId { get; protected set; }

    public int ToBusinessIdentityId { get; protected set; }

    public decimal Amount { get; protected set; }

    public decimal CashAmount { get; protected set; }

    public decimal CreditAmount { get; protected set; }

    public decimal PrepaymentAmount { get; protected set; }

    public decimal Commission { get; protected set; }

    public int Number { get; protected set; }

    public DateTime DueDate { get; protected set; }

    public InstallmentType Type { get; protected set; }

    public InstallmentStatus Status { get; protected set; }

    public string CheckSum { get; protected set; }

    public byte[] RowVersion { get; protected set; }

    public Tenant Tenant { get; protected set; }

    public Billing Billing { get; protected set; }

    public FinancialDocument FinancialDocument { get; protected set; }

    public BusinessIdentity FromBusinessIdentity { get; protected set; }

    public BusinessIdentity ToBusinessIdentity { get; protected set; }

    protected Installment()
    {

    }

    protected Installment(FinancialDocument financialDocument, int tenantId, int fromBusinessIdentityId,
        int toBusinessIdentityId, decimal amount, decimal cashAmount, decimal creditAmount, decimal prepaymentAmount,
        int number, DateTime dueDate, InstallmentType type)
    {
        Type = type;
        Number = number;
        DueDate = dueDate;
        TenantId = tenantId;
        CashAmount = cashAmount;
        CreditAmount = creditAmount;
        PrepaymentAmount = prepaymentAmount;
        Status = InstallmentStatus.Pending;
        FinancialDocument = financialDocument;
        ToBusinessIdentityId = toBusinessIdentityId;
        FromBusinessIdentityId = fromBusinessIdentityId;
        CalculateAmount(amount);
    }

    protected Installment(int tenantId, long financialDocumentId, int fromBusinessIdentityId,
        int toBusinessIdentityId, decimal amount, decimal cashAmount, decimal creditAmount, decimal prepaymentAmount, int number,
        DateTime dueDate, InstallmentType type)
    {
        Type = type;
        Number = number;
        DueDate = dueDate;
        TenantId = tenantId;
        CashAmount = cashAmount;
        CreditAmount = creditAmount;
        PrepaymentAmount = prepaymentAmount;
        Status = InstallmentStatus.Pending;
        FinancialDocumentId = financialDocumentId;
        ToBusinessIdentityId = toBusinessIdentityId;
        FromBusinessIdentityId = fromBusinessIdentityId;
        CalculateAmount(amount);
    }

    private void CalculateAmount(decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentValidationException(nameof(amount), "مبلغ قسط نمی تواند کوچک تر از صفر باشد");
        }

        Amount = amount;

        var compare = Amount == (CashAmount + CreditAmount + PrepaymentAmount);

        if (!compare)
        {
            throw new ArgumentValidationException(nameof(amount), "جمع ریز مبالغ با مبلغ کل قسط برابر نمی باشد");
        }
    }

    public void SetCommission(decimal commission)
    {
        if (commission <= 0)
        {
            throw new ArgumentValidationException(nameof(commission), "مبلغ کمیسیون قسط نمی تواند کوچک تر مساوی صفر باشد");
        }

        Commission = commission;
    }

    protected abstract void SetCheckSum();

    protected abstract void ValidateCheckSum();
}