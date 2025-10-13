using Domain.Base;
using Domain.Core.Entities.BillingPaymentAggregate;
using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Core.Entities.BillingAggregate;

public abstract class Billing : BaseEntity<long>
{
    public int TenantId { get; protected set; }

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

    public decimal AdditionsAmount { get; protected set; }

    public decimal DeductionsAmount { get; protected set; }

    public string? AdditionsDescription { get; protected set; }

    public string? DeductionsDescription { get; protected set; }

    public int GracePeriod { get; protected set; }

    public bool Transferred { get; protected set; }

    public int MainContractId { get; protected set; }

    public List<int> ContractIds { get; protected set; }

    public DateTime StartDate { get; protected set; }

    public DateTime EndDate { get; protected set; }

    public DateTime DueDate { get; protected set; }

    public long? DebtorId { get; protected set; }

    public long? CreditorId { get; protected set; }

    public byte[] RowVersion { get; protected set; }

    public string CheckSum { get; protected set; }

    public Tenant Tenant { get; protected set; }

    public Billing? Debtor { get; protected set; }

    public Billing? Creditor { get; protected set; }

    public BusinessIdentity FromBusinessIdentity { get; protected set; }

    public BusinessIdentity ToBusinessIdentity { get; protected set; }

    public List<BillingPayment> Payments { get; protected set; } = [];

    public ICollection<Billing> DebtorChildren { get; protected set; } = [];

    public ICollection<Billing> CreditorChildren { get; protected set; } = [];

    protected decimal PayableAmount => Amount - PaidAmount;

    protected decimal PaidAmount => Payments.Sum(p => p.Amount);

    protected Billing()
    {

    }

    protected Billing(int tenantId, int fromBusinessIdentityId, int toBusinessIdentityId, BillingType type,
        TimeInterval periodType, decimal previousDebitAmount, decimal previousCreditAmount,
        decimal previousPenaltyAmount, DateTime startDate, DateTime endDate, int gracePeriod, int mainContractId,
        List<int> contractIds, Billing? debtor = null, Billing? creditor = null)
    {
        Type = type;
        Debtor = debtor;
        Transferred = false;
        Creditor = creditor;
        TenantId = tenantId;
        AdditionsAmount = 0;
        DeductionsAmount = 0;
        PeriodType = periodType;
        GracePeriod = gracePeriod;
        MainContractId = mainContractId;
        PreviousDebitAmount = previousDebitAmount;
        PreviousCreditAmount = previousCreditAmount;
        ToBusinessIdentityId = toBusinessIdentityId;
        PreviousPenaltyAmount = previousPenaltyAmount;
        FromBusinessIdentityId = fromBusinessIdentityId;

        GenerateCode();
        SetContractIds(contractIds);
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

    public void Transfer()
    {
        Transferred = true;
    }

    public decimal GetPayableAmount()
    {
        return PayableAmount;
    }

    public decimal CalculatePayableAmount()
    {
        return Amount - Payments.Sum(p => p.Amount);
    }

    public void UpdateStatus(BillingStatus status)
    {
        ValidateCheckSum();
        Status = status;
        SetCheckSum();
        SetEditDateTime(DateTime.Now);
    }

    private void GenerateCode()
    {
        Code = Guid.NewGuid().ToString();
    }

    private void SetContractIds(List<int> contractIds)
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
        DueDate = endDate;
    }

    protected void SetFinalStatus()
    {
        if (Amount > 0)
        {
            Status = DueDate.AddDays(GracePeriod + 1) <= DateTime.Today ?
                BillingStatus.Overdue : BillingStatus.Issued;
        }
        else
        {
            Status = BillingStatus.Settled;
        }
    }

    protected abstract void SetCheckSum();

    protected abstract void ValidateCheckSum();

    protected abstract string GenerateCheckSum();
}