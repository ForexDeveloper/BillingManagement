using System;
using System.Collections.Generic;
using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.Shared.Exceptions;
using Domain.Core.Entities.WalletContractAggregate;

namespace Domain.Core.Entities.FinancierAggregate;

[Serializable]
public class Financier : BusinessIdentity
{
    #region Property

    public int TenantId { get; private set; }

    public Tenant Tenant { get; private set; }

    public string Name { get; private set; }

    public byte Type { get; private set; }

    public bool IsTenant { get; private set; }

    public List<WalletContractFinancier> WalletContractFinancier { get; private set; }

    #endregion #region Property

    private Financier()
    {
    }

    public Financier(int id, string name, int tenantId, byte type, bool isTenant = false)
    {
        Id = id;
        SetName(name);
        TenantId = tenantId;
        Type = type;
        IsTenant = isTenant;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentValidationException(nameof(name), $"{nameof(name)} is required");

        Name = name;
    }

    public void Update(string name, int tenantId, byte type)
    {
        TenantId = tenantId;
        SetName(name);
        Type = type;
        SetEditDateTime(DateTime.Now);
    }
}