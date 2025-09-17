using Domain.Base;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Core.Entities.WalletContractAggregate;

public class WalletContract : BaseEntity<int>
{
    public int TenantId { get; private set; }
    public Tenant Tenant { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public WalletContractStatus Status { get; private set; }
    public int? TenantIpgSettingId { get; private set; }
    public TenantIpgSetting TenantIpgSetting { get; private set; }
    public List<WalletContractGuarantor> WalletContractGuarantors { get; private set; } = [];
    public List<WalletContractFinancier> WalletContractFinanciers { get; private set; } = [];
    public List<WalletContractFacilitator> WalletContractFacilitators { get; private set; } = [];


    public WalletContract(int id, int tenantId, DateTime startDate, DateTime? endDate, WalletContractStatus status, int? tenantIpgSettingId)
    {
        Id = Id;
        TenantId = tenantId;
        TenantIpgSettingId = tenantIpgSettingId;
        StartDate = startDate;
        EndDate = endDate;
        Status = status;
        TenantIpgSettingId = TenantIpgSettingId;
    }

    public void SetWalletContractGuarantors(List<WalletContractGuarantor> walletContractGuarantors)
    {
        WalletContractGuarantors.AddRange(walletContractGuarantors);
    }

    public void SetWalletContractFinanciers(List<WalletContractFinancier> walletContractFinanciers)
    {
        WalletContractFinanciers.AddRange(walletContractFinanciers);
    }

    public void SetWalletContractFacilitators(List<WalletContractFacilitator> walletContractFacilitators)
    {
        WalletContractFacilitators.AddRange(walletContractFacilitators);
    }

    public void UpdateWalletContract(DateTime startDate, DateTime? endDate, int? tenantIpgSettingId, byte status)
    {
        Status = WalletContractStatus.Reviewing;
        StartDate = startDate;
        EndDate = endDate ?? endDate.Value;
        TenantIpgSettingId = tenantIpgSettingId;
        Status = (WalletContractStatus)status;
    }

}
