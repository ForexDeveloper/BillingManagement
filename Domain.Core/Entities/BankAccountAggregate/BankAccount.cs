using Domain.Base;
using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Enums;
using System;

namespace Domain.Core.Entities.BankAccountAggregate;

public class BankAccount : BaseEntity<int>
{
    private BankAccount()
    {
    }

    public BankAccount(bool isDefault, string ownerFullName, string lastNameAccountOwner,
        string iban, string accountNumber, int? bankId,
        int tenantId, int businessIdentityId)
    {
        IsDefault = isDefault;
        OwnerFullName = ownerFullName;
        Iban = iban;
        AccountNumber = accountNumber;
        BankId = bankId;
        TenantId = tenantId;
        BusinessIdentityId = businessIdentityId;
        Active();
    }

    public BankAccount(string iban, int tenantId, int businessIdentityId)
    {
        SetIban(iban);
        TenantId = tenantId;
        BusinessIdentityId = businessIdentityId;
        Active();
    }

    #region Property

    public string OwnerFullName { get; private set; }
    public string Iban { get; private set; }
    public string AccountNumber { get; private set; }
    public int? BankId { get; private set; }
    public bool IsDefault { get; private set; }
    public BankAccountStatus Status { get; private set; }
    public int BusinessIdentityId { get; private set; }
    public BusinessIdentity BusinessIdentity { get; private set; }
    public int TenantId { get; private set; }
    #endregion


    public void Active()
    {
        Status = BankAccountStatus.Active;
    }

    public void Deactive()
    {
        Status = BankAccountStatus.Inactive;
    }

    public void Delete()
    {
        IsDeleted = true;
    }

    private void SetIban(string iban)
    {
        if (!iban.IsValidIban())
        {
            throw new ArgumentValidationException(nameof(iban), "شماره شبا نمی تواند خالی باشد و باید با عبارت IR شروع شود");
        }

        Iban = iban;
    }
}
