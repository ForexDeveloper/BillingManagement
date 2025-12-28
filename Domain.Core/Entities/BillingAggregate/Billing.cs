using System;
using Domain.Base;
using System.Linq;
using Domain.Core.Enums;
using Domain.Core.Helper;
using System.Globalization;
using System.Collections.Generic;
using Domain.Core.Entities.Shared;
using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.BillingPaymentAggregate;

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

    public decimal TieredTransactionsAmount { get; protected set; }

    public decimal AdditionsAmount { get; protected set; }

    public decimal DeductionsAmount { get; protected set; }

    public string? AdditionsDescription { get; protected set; }

    public string? DeductionsDescription { get; protected set; }

    public int GracePeriod { get; protected set; }

    public bool Transferred { get; protected set; }

    public int MainContractId { get; protected set; }

    public List<int> ContractIds { get; protected set; }

    public DateTime StartDate { get; protected set; }

    public DateTime DueDate { get; protected set; }

    public DateTime PaymentDeadlineDate { get; set; }

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

    public List<TieredCalculatedLevel> TieredCalculatedLevels { get; protected set; }

    protected decimal PayableAmount => Amount - PaidAmount;

    protected decimal PaidAmount => Payments.Sum(p => p.Amount);

    protected Billing()
    {

    }

    protected Billing(int tenantId, int fromBusinessIdentityId, int toBusinessIdentityId, BillingType type,
        TimeInterval periodType, decimal previousDebitAmount, decimal previousCreditAmount,
        decimal previousPenaltyAmount, DateTime startDate, DateTime endDate, int gracePeriod, int mainContractId,
        List<int> contractIds, decimal tieredTransactionsAmount, List<TieredCalculatedLevel> tieredCalculatedLevels = null,
        Billing? debtor = null, Billing? creditor = null)
    {
        Type = type;
        Debtor = debtor;
        Creditor = creditor;
        Transferred = false;
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
        TieredCalculatedLevels = tieredCalculatedLevels;
        FromBusinessIdentityId = fromBusinessIdentityId;
        TieredTransactionsAmount = tieredTransactionsAmount;

        SetContractIds(contractIds);
        SetBillingDates(startDate, endDate);
        CalculatePaymentDeadlineDate();
    }

    public void Settle()
    {
        UpdateStatus(BillingStatus.Settled);
    }

    public void Overdue()
    {
        UpdateStatus(BillingStatus.Overdue);
    }

    public void PartialPay()
    {
        UpdateStatus(BillingStatus.PartiallyPaid);
    }

    public void Transfer()
    {
        Transferred = true;
        SetCheckSum();
        SetEditDateTime(DateTime.Now);
    }

    public decimal GetPayableAmount()
    {
        return PayableAmount;
    }

    public void SetAdditions(decimal additionsAmount, string? additionDescription)
    {
        if (additionsAmount < 0)
        {
            throw new ArgumentValidationException(nameof(additionsAmount), "مبلغ اضافات نمی تواند از 0 کوچکتر باشد");
        }

        if (Status == BillingStatus.Overdue)
        {
            throw new ArgumentValidationException(nameof(additionsAmount), "امکان ثبت اضافات برای صورتحساب معوق شده وجود ندارد");
        }

        if (PaymentDeadlineDate < DateTime.Today)
        {
            throw new ArgumentValidationException(nameof(additionsAmount), "امکان ثبت اضافات برای صورتحسابی که مهلت بازپرداخت آن گذشته است وجود ندارد");
        }

        var lastPayableAmount = PayableAmount;

        ValidateCheckSum();
        AdditionsAmount = additionsAmount;
        AdditionsDescription = additionDescription;
        CalculateAmount();

        if (PayableAmount > 0)
        {
            if (Status == BillingStatus.Settled)
            {
                Status = PaidAmount > 0 ? BillingStatus.PartiallyPaid : BillingStatus.Issued;
            }
        }
        else
        {
            Status = PayableAmount > lastPayableAmount ? BillingStatus.Settled
                : throw new ArgumentValidationException(nameof(additionsAmount),
                    "مبلغ اضافات نمی تواند باعث منفی شدن مبلغ قابل پرداخت صورتحساب شود");
        }

        SetCheckSum();
        SetEditDateTime(DateTime.Now);
    }

    public void SetDeductions(decimal deductionsAmount, string? deductionDescription)
    {
        if (deductionsAmount < 0)
        {
            throw new ArgumentValidationException(nameof(deductionsAmount), "مبلغ کسورات نمی تواند از 0 کوچکتر باشد");
        }

        if (Status is BillingStatus.Overdue)
        {
            throw new ArgumentValidationException(nameof(deductionsAmount), "امکان ثبت کسورات برای صورتحساب معوق شده وجود ندارد");
        }

        if (PaymentDeadlineDate < DateTime.Today)
        {
            throw new ArgumentValidationException(nameof(deductionsAmount), "امکان ثبت کسورات برای صورتحسابی که مهلت بازپرداخت آن گذشته است وجود ندارد");
        }

        var lastPayableAmount = PayableAmount;

        ValidateCheckSum();
        DeductionsAmount = deductionsAmount;
        DeductionsDescription = deductionDescription;
        CalculateAmount();

        if (PayableAmount > 0)
        {
            if (Status == BillingStatus.Settled)
            {
                Status = PaidAmount > 0 ? BillingStatus.PartiallyPaid : BillingStatus.Issued;
            }
        }
        else
        {
            Status = PayableAmount == 0 ? BillingStatus.Settled
                : throw new ArgumentValidationException(nameof(deductionsAmount),
                    "مبلغ کسورات نمی تواند از مبلغ قابل پرداخت صورتحساب بیشتر باشد");
        }

        SetCheckSum();
        SetEditDateTime(DateTime.Now);
    }

    public void AddBillingPayment(long paymentId, decimal amount, DateTime paymentDate)
    {
        Payments.Add(new BillingPayment(paymentId, amount, paymentDate));
    }

    protected void Configure()
    {
        CalculateAmount();
        InitiateStatus();
        GenerateCode();
        SetCheckSum();
    }

    protected abstract void CalculateAmount();

    protected abstract string GenerateCodePrefix();

    protected abstract void SetCheckSum();

    protected abstract void ValidateCheckSum();

    private void GenerateCode()
    {
        var prefix = GenerateCodePrefix();

        var pc = new PersianCalendar();
        var year2Digits = pc.GetYear(CreatedDateTime).GetLast2Digits();
        var month2Digits = pc.GetMonth(CreatedDateTime).GetLast2Digits();
        var georgianYear2Digits = CreatedDateTime.Year.GetLast2Digits();
        var georgianMonth2Digits = CreatedDateTime.Month.GetLast2Digits();
        var georgianDay2Digits = CreatedDateTime.Day.GetLast2Digits();
        var georgianHour2Digits = CreatedDateTime.Hour.GetLast2Digits();
        var georgianMinute2Digits = CreatedDateTime.Minute.GetLast2Digits();
        var georgianSecond2Digits = CreatedDateTime.Second.GetLast2Digits();
        var georgianMilliSecond2Digits = CreatedDateTime.Millisecond.GetLast2Digits();
        var georgianMicroSecond2Digits = CreatedDateTime.Microsecond.GetLast2Digits();

        Code = $"{prefix}{year2Digits}{month2Digits}{georgianYear2Digits}{georgianMonth2Digits}{georgianDay2Digits}{georgianHour2Digits}{georgianMinute2Digits}{georgianSecond2Digits}{georgianMilliSecond2Digits}{georgianMicroSecond2Digits}";
    }

    private void UpdateStatus(BillingStatus status)
    {
        ValidateCheckSum();
        Status = status;
        SetCheckSum();
        SetEditDateTime(DateTime.Now);
    }

    private void InitiateStatus()
    {
        if (Amount > 0)
        {
            Status = PaymentDeadlineDate < DateTime.Today ? BillingStatus.Overdue : BillingStatus.Issued;
        }
        else
        {
            Status = BillingStatus.Settled;
        }
    }

    private void SetContractIds(List<int> contractIds)
    {
        if (contractIds == null || contractIds.Count == 0)
        {
            throw new ArgumentValidationException(nameof(contractIds), "لیست شناسه قرارداد های صورتحساب نمی تواند خالی باشد");
        }

        ContractIds = contractIds;
    }

    private void SetBillingDates(DateTime startDate, DateTime endDate)
    {
        if (startDate >= endDate)
        {
            throw new ArgumentValidationException(nameof(startDate), "تاریخ شروع صورتحساب نمی تواند از تاریخ پایان آن بزرگتر باشد");
        }

        StartDate = startDate;
        DueDate = endDate;
    }

    private void CalculatePaymentDeadlineDate()
    {
        PaymentDeadlineDate = DueDate.AddDays(GracePeriod);
    }
}