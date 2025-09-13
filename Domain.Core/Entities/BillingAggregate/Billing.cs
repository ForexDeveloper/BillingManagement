using Domain.Base;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Enums;
using Domain.Core.Helper;
using System;
using System.Collections.Generic;

namespace Domain.Core.Entities.BillingAggregate;

public class Billing : BaseEntity<long>
{
    #region Property 
    public int FromAccountId { get; private set; }
    public Account FromAccount { get; private set; }
    public int ToBusinessIdentityId { get; private set; }
    public BusinessIdentity ToBusinessIdentity { get; private set; }
    public int TenantId { get; private set; }
    public Tenant Tenant { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public decimal Amount { get; private set; }
    public decimal PreviousDebitAmount { get; private set; } // بستانکاری
    public decimal PreviousCreditAmount { get; private set; }
    public decimal PreviousPenaltyAmount { get; private set; }
    public string CheckSum { get; private set; }
    public BillingState State { get; private set; }
    public BillingCategory Category { get; private set; }
    public int GracePeriod { get; private set; }
    public WalletSettlementType SettlementType { get; private set; }
    public byte[] RowVersion { get; private set; }
    public ICollection<BillingPayment> BillingPayments { get; private set; } = [];
    public ICollection<BillingInstallment> BillingInstallments { get; private set; } = [];

    #endregion

    private Billing()
    {

    }

    public Billing(Account fromAccount, int toBusinessIdentityId, int tenantId, DateTime startDate, DateTime endDate,
        decimal amount, decimal previousDebitAmount = 0, int gracePeriod = 0, WalletSettlementType settlementType = WalletSettlementType.Cash)
    {
        FromAccount = fromAccount;
        FromAccountId = FromAccount.Id;
        ToBusinessIdentityId = toBusinessIdentityId;
        TenantId = tenantId;
        StartDate = startDate;
        EndDate = endDate;
        State = BillingState.Pending;
        PreviousDebitAmount = previousDebitAmount;
        PreviousCreditAmount = 0;
        PreviousPenaltyAmount = 0;
        GracePeriod = gracePeriod;
        SettlementType = settlementType;
        SetAmount(amount);
        SetCheckSum();
    }

    public void SetAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentValidationException(nameof(amount), "مبلغ صورتحساب نمی تواند کوچک تر مساوی صفر باشد");

        Amount = amount;
    }

    public void SetCheckSum()
    {
        CheckSum = $"{FromAccountId}{ToBusinessIdentityId}{TenantId}{Amount:F10}{PreviousDebitAmount:F10}{PreviousCreditAmount:F10}{PreviousPenaltyAmount:F10}{GracePeriod}{State}{StartDate:yyyy-MM-ddTHH:mm:ss}{EndDate:yyyy-MM-ddTHH:mm:ss}{CreatedDateTime:yyyy-MM-ddTHH:mm:ss}".Hash();
    }

    public void AddBillingPayment(BillingPayment billingPayment)
    {
        BillingPayments.Add(billingPayment);
    }

    public void AddBillingInstallment(BillingInstallment billingInstallment)
    {
        BillingInstallments.Add(billingInstallment);
    }

    public void UpdateState(BillingState state)
    {
        ValidateCheckSum();

        State = state;
        SetCheckSum();
    }

    public void UpdatePreviousDebit(decimal previousDebitAmount)
    {
        ValidateCheckSum();

        PreviousDebitAmount += previousDebitAmount;
        SetCheckSum();
    }

    public void UpdatePreviousCredit(decimal previousCreditAmount)
    {
        ValidateCheckSum();

        PreviousCreditAmount += previousCreditAmount;
        SetCheckSum();
    }

    public void UpdatePreviousPenaltyAmount(decimal previousPenaltyAmount)
    {
        ValidateCheckSum();

        PreviousPenaltyAmount += previousPenaltyAmount;
        SetCheckSum();
    }

    public void UpdatePreviousCreditAndDebitAndPenalty(decimal previousDebitAmount, decimal previousCreditAmount,
        decimal previousPenaltyAmount)
    {
        ValidateCheckSum();

        PreviousDebitAmount += previousDebitAmount;
        PreviousCreditAmount += previousCreditAmount;
        PreviousPenaltyAmount += previousPenaltyAmount;
        SetCheckSum();
    }

    public void ValidateCheckSum()
    {
        string comperedTo = $"{FromAccountId}{ToBusinessIdentityId}{TenantId}{Amount:F10}{PreviousDebitAmount:F10}{PreviousCreditAmount:F10}{PreviousPenaltyAmount:F10}{GracePeriod}{State}{StartDate:yyyy-MM-ddTHH:mm:ss}{EndDate:yyyy-MM-ddTHH:mm:ss}{CreatedDateTime:yyyy-MM-ddTHH:mm:ss}";

        if (!comperedTo.Validate(CheckSum))
            throw new ArgumentValidationException("InconsistentData", "اطلاعات موجود در دیتابیس صحیح نمی باشد.");
    }

    public void UpdateDate(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
        SetCheckSum();
    }
}
