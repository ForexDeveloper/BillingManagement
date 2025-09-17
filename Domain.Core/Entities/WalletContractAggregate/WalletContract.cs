using Domain.Base;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Enums;
using System;

namespace Domain.Core.Entities.WalletContractAggregate;

public class WalletContract : BaseEntity<int>
{
    public int TenantId { get; private set; }
    public Tenant Tenant { get; private set; }
    public string ContractNumber { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public WalletContractStatus Status { get; private set; }
    public int? TenantIpgSettingId { get; private set; }
    public TenantIpgSetting TenantIpgSetting { get; private set; }
}
