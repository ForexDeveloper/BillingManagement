using Domain.Base;
using System.Collections.Generic;
using Domain.Core.Entities.TenantPlatformContractAggregate;

namespace Domain.Core.Entities.TenantAggregate;

public class TenantIpgSetting : BaseEntity<int>
{
    public string Title { get; set; }

    public int TenantId { get; private set; }

    public Tenant Tenant { get; private set; }

    public byte IpgType { get; set; }

    public bool IsActive { get; private set; }

    public List<TenantPlatformContract> TenantPlatformContracts { get; private set; }

    private TenantIpgSetting()
    {
    }

    public TenantIpgSetting(int id, string title, int tenantId, byte ipgType, bool isActive)
    {
        Id = id;
        Title = title;
        TenantId = tenantId;
        IpgType = ipgType;
        IsActive = isActive;
    }

    public void Update(string title, int tenantId, byte ipgType, bool isActive)
    {
        Title = title;
        TenantId = tenantId;
        IpgType = ipgType;
        IsActive = isActive;
    }
}