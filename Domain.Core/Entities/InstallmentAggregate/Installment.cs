using Domain.Base;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.TransactionAggregate;
using Domain.Core.Enums;
using Domain.Core.Helper;
using System;
using System.Collections.Generic;

namespace Domain.Core.Entities.InstallmentAggregate;

public class Installment : BaseEntity<long>
{
    #region Property
    public int FromAccountId { get; private set; }
    public Account FromAccount { get; private set; }

    public int ToAccountId { get; private set; }
    public Account ToAccount { get; private set; }

    public long? TransactionId { get; private set; }
    public Transaction Transaction { get; private set; }
    public int TenantId { get; private set; }
    public Tenant Tenant { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public InstallmentType Type { get; private set; }
    public InstallmentState State { get; private set; }
    public InstallmentCategory Category { get; private set; }
    public bool HasBilling { get; private set; }
    public DateTime? LastPenaltyCalculationDate { get; private set; }
    public decimal PaidAmount { get; private set; }
    public long? ParentId { get; private set; }
    public Installment Parent { get; private set; }
    public string CheckSum { get; private set; }
    public int? Number { get; private set; }
    public int GracePeriod { get; private set; }
    public WalletSettlementType SettlementType { get; private set; }
    public string SayyadIdentifier { get; private set; }
    public byte[] RowVersion { get; private set; }
    public ICollection<BillingInstallment> BillingInstallments { get; private set; }

    #endregion

    private Installment() { }

    public Installment(int fromAccountId, int toAccountId, int tenantId, decimal amount,
       DateTime dueDate, InstallmentType type, InstallmentCategory category, DateTime startDate, Transaction transaction = null,
        int gracePeriod = 0, WalletSettlementType settlementType = WalletSettlementType.Cash)
    {
        FromAccountId = fromAccountId;
        ToAccountId = toAccountId;
        Transaction = transaction;
        TenantId = tenantId;
        DueDate = dueDate;
        Type = type;
        Category = category;
        State = InstallmentState.Pending;
        Category = category;
        StartDate = startDate;
        PaidAmount = 0;
        GracePeriod = gracePeriod;
        SettlementType = settlementType;
        SetAmount(amount);
        SetCheckSum();
    }

    public Installment(Account fromAccount, int toAccountId, int tenantId, decimal amount,
       DateTime dueDate, InstallmentType type, InstallmentCategory category, DateTime startDate, Transaction transaction = null,
        int gracePeriod = 0, WalletSettlementType settlementType = WalletSettlementType.Cash, string sayyadIdentifier = null)
    {
        FromAccount = fromAccount;
        FromAccountId = fromAccount.Id;
        ToAccountId = toAccountId;
        Transaction = transaction;
        TenantId = tenantId;
        DueDate = dueDate;
        Type = type;
        Category = category;
        State = InstallmentState.Pending;
        Category = category;
        StartDate = startDate;
        PaidAmount = 0;
        GracePeriod = gracePeriod;
        SettlementType = settlementType;
        SetAmount(amount);
        SetCheckSum();
        SayyadIdentifier = sayyadIdentifier;
    }

    public void SetAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentValidationException(nameof(amount), "مبلغ قسط نمی تواند کوچک تر مساوی صفر باشد");

        Amount = amount;
    }

    public void SetCheckSum()
    {
        CheckSum = $"{FromAccountId}{ToAccountId}{Amount:F10}{PaidAmount:F10}{GracePeriod}{State}{StartDate:yyyy-MM-ddTHH:mm:ss}{CreatedDateTime:yyyy-MM-ddTHH:mm:ss}".Hash();
    }

    public void SetParentId(long? parentId)
    {
        ParentId = parentId;
    }

    public void SetParent(Installment installment)
    {
        Parent = installment;
    }

    public void SetPaidAmount(decimal paidAmount)
    {
        ValidateCheckSum();

        PaidAmount = paidAmount;
        SetCheckSum();
    }

    public void UpdatePaidAmount(decimal paidAmount)
    {
        ValidateCheckSum();

        PaidAmount += paidAmount;
        SetCheckSum();
    }

    public void UpdateStatus(InstallmentState state)
    {
        ValidateCheckSum();

        State = state;
        SetCheckSum();
    }

    public void SetOverdue()
    {
        ValidateCheckSum();

        State = InstallmentState.Overdue;
        LastPenaltyCalculationDate = DateTime.Now;
        SetCheckSum();
    }

    public void SetLastPenaltyCalculationDate(DateTime dateTime)
    {
        LastPenaltyCalculationDate = dateTime;
    }

    public void UpdateHasBilling(bool hasBilling)
    {
        HasBilling = hasBilling;
    }

    public void SetNumber(int number)
    {
        if (Type == InstallmentType.Installment)
            Number = number;
    }

    public void ValidateCheckSum()
    {
        string comperedTo = $"{FromAccountId}{ToAccountId}{Amount:F10}{PaidAmount:F10}{GracePeriod}{State}{StartDate:yyyy-MM-ddTHH:mm:ss}{CreatedDateTime:yyyy-MM-ddTHH:mm:ss}";

        if (!comperedTo.Validate(CheckSum))
            throw new ArgumentValidationException("InconsistentData", "اطلاعات موجود در دیتابیس صحیح نمی باشد.");
    }

    public void UpdateDate(DateTime startDate, DateTime dueDate)
    {
        StartDate = startDate;
        DueDate = dueDate;
        SetCheckSum();
    }
}
