using Domain.Base;
using Domain.Core.AggregateRoots.WalletConfigurationAggregate;
using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Enums;
using Domain.Core.Helper;

namespace Domain.Core.Entities.AccountAggregate;

public class Account : BaseEntity<int>
{
    #region Property
    public int DefaultCurrencyTypeId => 1;
    public AccountStatus Status { get; private set; }

    public int BusinessIdentityId { get; private set; }
    public BusinessIdentity BusinessIdentity { get; private set; }
    public int TenantId { get; private set; }
    public Tenant Tenant { get; private set; }
    public int CurrencyId { get; private set; }
    public CurrencyType Currency { get; private set; }

    public AccountType Type { get; private set; }

    public decimal Balance { get; private set; } //
    public decimal NonWithDrawableBalance { get; private set; }
    public decimal WithDrawableBalance => Balance - NonWithDrawableBalance;
    public string CheckSum { get; private set; }

    #endregion Property

    private Account() { }

    public Account(int businessIdentityId,
        int tenantId, AccountType type, decimal balance, decimal nonWithDrawableBalance = 0, int? currencyId = null)
    {
        Status = AccountStatus.Active;
        BusinessIdentityId = businessIdentityId;
        TenantId = tenantId;
        CurrencyId = currencyId is 0 or null ? DefaultCurrencyTypeId : currencyId.Value;
        Type = type;
        Balance = balance;
        NonWithDrawableBalance += nonWithDrawableBalance;
        SetCheckSum();
    }

    public void SetCheckSum()
    {
        CheckSum = $"{BusinessIdentityId}{Type}{CurrencyId}{Balance:F10}{NonWithDrawableBalance:F10}{CreatedDateTime:yyyy-MM-ddTHH:mm:ss}".Hash();
    }

    public void IncreaseBalance(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentValidationException(nameof(amount), "مبلغ نمی تواند کوچک تر مساوی صفر باشد");

        ValidateCheckSum();
        Balance += amount;
        SetCheckSum();
    }

    public void DecreaseBalance(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentValidationException(nameof(amount), "مبلغ نمی تواند کوچک تر مساوی صفر باشد");

        ValidateCheckSum();
        Balance -= amount;
        SetCheckSum();
    }

    public void SetNonWithDrawable(decimal nonWithDrawable)
    {
        ValidateCheckSum();

        Balance += nonWithDrawable;
        NonWithDrawableBalance += nonWithDrawable;
    }

    public void ValidateCheckSum()
    {
        string comperedTo = $"{BusinessIdentityId}{Type}{CurrencyId}{Balance:F10}{NonWithDrawableBalance:F10}{CreatedDateTime:yyyy-MM-ddTHH:mm:ss}";

        if (!comperedTo.Validate(CheckSum))
            throw new ArgumentValidationException("InconsistentData", $"اطلاعات موجود برای حساب {Id} در دیتابیس صحیح نمی باشد.");
    }
}
