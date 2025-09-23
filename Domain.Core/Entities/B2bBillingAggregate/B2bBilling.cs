using System;
using System.Linq;
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

    public TimeInterval PeriodType { get; protected set; }

    public decimal Amount { get; protected set; }

    public decimal PreviousDebitAmount { get; protected set; }

    public decimal PreviousCreditAmount { get; protected set; }

    public decimal PreviousPenaltyAmount { get; protected set; }

    public int GracePeriod { get; protected set; }

    public IEnumerable<int> ContractIds { get; protected set; }

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

    public ICollection<B2bBilling> Children { get; protected set; } = [];

    protected decimal PayableAmount => Amount - PaidAmount;

    protected decimal PaidAmount => Payments.Sum(p => p.Amount);

    protected B2bBilling()
    {

    }

    protected B2bBilling(int tenantId, int fromBusinessIdentityId, int toBusinessIdentityId, BillingType type,
        TimeInterval periodType, decimal previousDebitAmount, decimal previousCreditAmount,
        decimal previousPenaltyAmount, DateTime startDate, DateTime endDate, int gracePeriod,
        IEnumerable<int> contractIds, B2bBilling? parent = null)
    {
        Type = type;
        Parent = parent;
        TenantId = tenantId;
        PeriodType = periodType;
        GracePeriod = gracePeriod;
        PreviousDebitAmount = previousDebitAmount;
        PreviousCreditAmount = previousCreditAmount;
        ToBusinessIdentityId = toBusinessIdentityId;
        PreviousPenaltyAmount = previousPenaltyAmount;
        FromBusinessIdentityId = fromBusinessIdentityId;

        GenerateCode();
        SetContractIds(contractIds);
        SetDueDate(endDate, gracePeriod);
        SetBillingRanges(startDate, endDate);
    }

    protected B2bBilling(int tenantId, int fromBusinessIdentityId, int toBusinessIdentityId, BillingType type,
        TimeInterval periodType, decimal previousDebitAmount, decimal previousCreditAmount,
        decimal previousPenaltyAmount, DateTime startDate, DateTime endDate, int gracePeriod,
        IEnumerable<int> contractIds, IEnumerable<B2bInstallment> installments, B2bBilling? parent = null)
    {
        Type = type;
        Parent = parent;
        TenantId = tenantId;
        PeriodType = periodType;
        GracePeriod = gracePeriod;
        PreviousDebitAmount = previousDebitAmount;
        PreviousCreditAmount = previousCreditAmount;
        ToBusinessIdentityId = toBusinessIdentityId;
        PreviousPenaltyAmount = previousPenaltyAmount;
        FromBusinessIdentityId = fromBusinessIdentityId;

        GenerateCode();
        SetContractIds(contractIds);
        AddInstallments(installments);
        SetDueDate(endDate, gracePeriod);
        SetBillingRanges(startDate, endDate);
    }

    public void Settle()
    {
        UpdateStatus(BillingStatus.Settled);
    }

    public void Overdue()
    {
        UpdateStatus(BillingStatus.Overdue);
    }

    public decimal GetDebitAmount()
    {
        return PayableAmount;
    }

    public decimal CalculateDebitAmount()
    {
        return Amount - Payments.Sum(p => p.Amount);
    }

    private void UpdateStatus(BillingStatus status)
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

    private void SetContractIds(IEnumerable<int> contractIds)
    {
        if (contractIds == null || contractIds.Any() == false)
        {
            throw new ArgumentValidationException(nameof(contractIds), "لیست شناسه قرارداد های صورتحساب نمی تواند خالی باشد");
        }

        ContractIds = contractIds;
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

    protected void SettleOrIssue()
    {
        Status = Amount == 0 ? BillingStatus.Settled : BillingStatus.Issued;
    }

    protected abstract void SetCheckSum();

    protected abstract void ValidateCheckSum();
}