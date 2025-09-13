using System;
using Domain.Base;
using Domain.Core.Enums;
using System.Collections.Generic;
using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.B2bInstallmentAggregate;
using Domain.Core.Entities.B2bBillingPaymentAggregate;

namespace Domain.Core.Entities.B2bBillingAggregate;

public abstract class B2bBilling : BaseEntity<long>
{
    public int TenantId { get; protected set; }

    public long? ParentId { get; protected set; }

    public int FromBusinessIdentityId { get; protected set; }

    public int ToBusinessIdentityId { get; protected set; }

    public string Code { get; protected set; }

    public BillingStatus Status { get; protected set; }

    public BillingType Type { get; protected set; }

    public decimal Amount { get; protected set; }

    public decimal PreviousDebitAmount { get; protected set; }

    public decimal PreviousCreditAmount { get; protected set; }

    public decimal PreviousPenaltyAmount { get; protected set; }

    public int GracePeriod { get; protected set; }

    public DateTime StartDate { get; protected set; }

    public DateTime EndDate { get; protected set; }

    public DateTime DueDate { get; protected set; }

    public byte[] RowVersion { get; protected set; }

    public string CheckSum { get; protected set; }

    public Tenant Tenant { get; protected set; }

    public B2bBilling? Parent { get; protected set; }

    public BusinessIdentity FromBusinessIdentity { get; protected set; }

    public BusinessIdentity ToBusinessIdentity { get; protected set; }

    public List<B2bInstallment> Installments { get; protected set; } = [];

    public List<B2bBillingPayment> Payments { get; protected set; } = [];

    protected B2bBilling()
    {

    }

    protected B2bBilling(int tenantId, int fromBusinessIdentityId, int toBusinessIdentityId, BillingType type,
        decimal amount, decimal previousDebitAmount, decimal previousCreditAmount, decimal previousPenaltyAmount,
        DateTime startDate, DateTime endDate, int gracePeriod, B2bBilling? parent = null)
    {
        Type = type;
        Parent = parent;
        Amount = amount;
        TenantId = tenantId;
        GracePeriod = gracePeriod;
        Status = BillingStatus.Issued;
        PreviousDebitAmount = previousDebitAmount;
        PreviousCreditAmount = previousCreditAmount;
        ToBusinessIdentityId = toBusinessIdentityId;
        PreviousPenaltyAmount = previousPenaltyAmount;
        FromBusinessIdentityId = fromBusinessIdentityId;

        GenerateCode();
        SetAmount(amount);
        SetDueDate(endDate, gracePeriod);
        SetBillingRanges(startDate, endDate);
    }

    protected B2bBilling(int tenantId, int fromBusinessIdentityId, int toBusinessIdentityId, BillingType type,
        decimal amount, decimal previousDebitAmount, decimal previousCreditAmount, decimal previousPenaltyAmount,
        DateTime startDate, DateTime endDate, int gracePeriod, IEnumerable<B2bInstallment> installments,
        B2bBilling? parent = null)
    {
        Type = type;
        Parent = parent;
        Amount = amount;
        TenantId = tenantId;
        GracePeriod = gracePeriod;
        Status = BillingStatus.Issued;
        PreviousDebitAmount = previousDebitAmount;
        PreviousCreditAmount = previousCreditAmount;
        ToBusinessIdentityId = toBusinessIdentityId;
        PreviousPenaltyAmount = previousPenaltyAmount;
        FromBusinessIdentityId = fromBusinessIdentityId;

        GenerateCode();
        SetAmount(amount);
        AddInstallments(installments);
        SetDueDate(endDate, gracePeriod);
        SetBillingRanges(startDate, endDate);
    }



    public void Overdue()
    {
        ValidateCheckSum();
        Status = BillingStatus.Overdue;
        SetCheckSum();
        SetEditDateTime(DateTime.Now);
    }

    public void UpdateStatus(BillingStatus status)
    {
        ValidateCheckSum();
        Status = status;
        SetCheckSum();
        SetEditDateTime(DateTime.Now);
    }

    private void SetDueDate(DateTime endDate, int gracePeriod)
    {
        DueDate = endDate.AddDays(gracePeriod);
    }

    private void GenerateCode()
    {
        Code = Guid.NewGuid().ToString();
    }

    private void SetAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentValidationException(nameof(amount), "مبلغ صورتحساب نمی تواند کوچک تر مساوی صفر باشد");
        }

        Amount = amount;
    }

    private void SetBillingRanges(DateTime startDate, DateTime endDate)
    {
        if (startDate >= endDate)
        {
            throw new ArgumentValidationException(nameof(startDate), "تاریخ شروع صورتحساب نمی تواند از تاریخ پایان آن بزرگتر باشد");
        }

        StartDate = startDate;
        EndDate = endDate;
    }

    private void AddInstallments(IEnumerable<B2bInstallment> installments)
    {
        Installments.AddRange(installments);
    }

    protected abstract void SetCheckSum();

    protected abstract void ValidateCheckSum();
}