using System;
using Domain.Base;
using Domain.Core.Enums;
using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.B2bBillingAggregate;
using Domain.Core.Entities.FinancialDocumentAggregate;

namespace Domain.Core.Entities.B2bInstallmentAggregate;

public abstract class B2bInstallment : BaseEntity<long>
{
    public int TenantId { get; protected set; }

    public long FinancialDocumentId { get; protected set; }

    public long? BillingId { get; protected set; }

    public int FromBusinessIdentityId { get; protected set; }

    public int ToBusinessIdentityId { get; protected set; }

    public decimal Amount { get; protected set; }

    public decimal Commission { get; protected set; }

    public int Number { get; protected set; }

    public DateTime DueDate { get; protected set; }

    public B2bInstallmentType Type { get; protected set; }

    public InstallmentStatus Status { get; protected set; }

    //public InstallmentCategory Category { get; protected set; }

    public string CheckSum { get; protected set; }

    public byte[] RowVersion { get; protected set; }

    public Tenant Tenant { get; protected set; }

    public B2bBilling Billing { get; protected set; }

    public FinancialDocument FinancialDocument { get; protected set; }

    public BusinessIdentity FromBusinessIdentity { get; protected set; }

    public BusinessIdentity ToBusinessIdentity { get; protected set; }

    protected B2bInstallment()
    {

    }

    protected B2bInstallment(FinancialDocument financialDocument, int tenantId, int fromBusinessIdentityId,
        int toBusinessIdentityId, decimal amount, int number,
        DateTime dueDate, B2bInstallmentType type)
    {
        Type = type;
        Amount = amount;
        Number = number;
        DueDate = dueDate;
        TenantId = tenantId;
        Status = InstallmentStatus.Pending;
        FinancialDocument = financialDocument;
        ToBusinessIdentityId = toBusinessIdentityId;
        FromBusinessIdentityId = fromBusinessIdentityId;
        SetAmount(amount);
    }

    protected B2bInstallment(int tenantId, long financialDocumentId, int fromBusinessIdentityId,
        int toBusinessIdentityId, decimal amount, int number,
        DateTime dueDate, B2bInstallmentType type)
    {
        Type = type;
        Number = number;
        DueDate = dueDate;
        TenantId = tenantId;
        Status = InstallmentStatus.Pending;
        FinancialDocumentId = financialDocumentId;
        ToBusinessIdentityId = toBusinessIdentityId;
        FromBusinessIdentityId = fromBusinessIdentityId;
        SetAmount(amount);
    }

    private void SetAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentValidationException(nameof(amount), "مبلغ قسط نمی تواند کوچک تر مساوی صفر باشد");
        }

        Amount = amount;
    }

    protected abstract void SetCheckSum();

    protected abstract void ValidateCheckSum();
}