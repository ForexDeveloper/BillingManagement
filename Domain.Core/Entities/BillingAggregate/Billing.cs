using Domain.Base;
using Domain.Core.Entities.BillingPaymentAggregate;
using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.Shared;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Enums;
using Domain.Core.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
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

    protected Billing(int tenantId, TimeInterval periodType, decimal previousDebitAmount, decimal previousCreditAmount,
        decimal previousPenaltyAmount, DateTime startDate, DateTime endDate, int gracePeriod, int mainContractId,
        List<int> contractIds, List<TieredCalculatedLevel> tieredCalculatedLevels = null, Billing? debtor = null,
        Billing? creditor = null)
    {
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
        PreviousPenaltyAmount = previousPenaltyAmount;
        TieredCalculatedLevels = tieredCalculatedLevels;

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

    public void SetAdditions(decimal additions, string? additionDescription)
    {
        if (additions < 0)
        {
            throw new ArgumentValidationException(nameof(additions), "مبلغ اضافات نمی تواند از 0 کوچکتر باشد");
        }

        AdditionsAmount = additions;
        AdditionsDescription = additionDescription;
        CalculateAmount();
        SetCheckSum();
    }

    public void SetDeductions(decimal deductions, string? deductionDescription)
    {
        if (deductions < 0)
        {
            throw new ArgumentValidationException(nameof(deductions), "مبلغ کسورات نمی تواند از 0 کوچکتر باشد");
        }

        Amount += DeductionsAmount;

        if (Amount < deductions)
        {
            throw new ArgumentValidationException(nameof(deductions), "مبلغ کسورات نمی تواند از مبلغ کل صورتحساب بیشتر باشد");
        }

        DeductionsDescription = deductionDescription;
        DeductionsAmount = deductions;
        CalculateAmount();
        SetFinalStatus();
        SetCheckSum();
    }

    public void AddBillingPayment(long billingId, long paymentId, decimal amount, DateTime paymentDate)
    {
        Payments.Add(new BillingPayment(billingId, paymentId, amount, paymentDate));
    }

    protected void Configure(int fromBusinessIdentityId, int toBusinessIdentityId)
    {
        CalculateAmount();
        SetBillingType();
        SetAbsoluteAmount();
        SetFinalStatus();
        var prefix = GenerateCodePrefix();
        GenerateCode(prefix);
        SetIdentity(fromBusinessIdentityId, toBusinessIdentityId);
        SetCheckSum();
    }

    protected abstract void CalculateAmount();

    protected abstract void SetBillingType();

    protected abstract string GenerateCodePrefix();

    protected abstract void SetCheckSum();

    protected abstract void ValidateCheckSum();

    protected abstract void SetIdentity(int fromBusinessIdentityId, int toBusinessIdentityId);

    private void GenerateCode(string prefix)
    {
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

    private void SetFinalStatus()
    {
        if (Amount == 0)
        {
            Status = BillingStatus.Settled;
        }
        else
        {
            Status = PaymentDeadlineDate < DateTime.Today ? BillingStatus.Overdue : BillingStatus.Issued;
        }
    }

    private void SetAbsoluteAmount()
    {
        Amount = Math.Abs(Amount);
    }

    private void SetContractIds(List<int> contractIds)
    {
        if (contractIds == null || contractIds.Any() == false)
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