using Domain.Base;
using Domain.Core.Entities.OrganizationAggregate;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Core.Entities.WalletContractAggregate
{
    public class WalletContract : BaseEntity<int>
    {
        #region Property
        public int TenantId { get; private set; }
        public Tenant Tenant { get; private set; }
        public string ContractNumber { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public WalletContractStatus Status { get; private set; }
        public DateTime? ChangeStatusDate { get; private set; }
        public int OrganizationId { get; private set; }
        public Organization Organization { get; private set; }
        public int? TenantIpgSettingId { get; private set; }
        public TenantIpgSetting TenantIpgSetting { get; private set; }
        public int? GrantingProcessId { get; private set; }
        public int? ParentId { get; private set; }
        public WalletContract Parent { get; private set; }

        public int? RootParentId { get; private set; }
        public WalletContract RootParent { get; private set; }
        public List<WalletContractPlan> WalletContractPlans { get; private set; } = [];
        public List<WalletContractBusinessIdentity> WalletContractCustomers { get; private set; } = [];
        public List<WalletContractGuarantor> WalletContractGuarantors { get; private set; } = [];
        public List<WalletContractFinancier> WalletContractFinanciers { get; private set; } = [];
        public List<WalletContractFacilitator> WalletContractFacilitators { get; private set; } = [];
        public List<WalletContractRejectionReason> WalletContractRejectionReasons { get; private set; } = [];
        #endregion

        private WalletContract() { }

        public WalletContract(int tenantId, int? tenantIpgSettingId, int organizationId, DateTime startDate, DateTime? endDate, WalletContractStatus status = WalletContractStatus.Reviewing)
        {
            TenantId = tenantId;
            TenantIpgSettingId = tenantIpgSettingId;
            Status = status;
            OrganizationId = organizationId;
            StartDate = startDate;
            SetEndDate(endDate);
            SetChangeStatusDate();
        }

        public void UpdateWalletContract(int? tenantIpgSettingId, DateTime startDate, DateTime? endDate)
        {
            TenantIpgSettingId = tenantIpgSettingId;
            Status = WalletContractStatus.Reviewing;
            StartDate = startDate;
            SetEndDate(endDate);
            SetEditDateTime(DateTime.Now);
        }

        public void SetWalletContractBusinessIdentities(List<WalletContractBusinessIdentity> contractCustomers)
        {
            WalletContractCustomers.AddRange(contractCustomers);
        }

        public void SetWalletContractBusinessIdentity(WalletContractBusinessIdentity contractCustomer)
        {
            WalletContractCustomers.Add(contractCustomer);
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

        public void SetWalletContractPlans(List<WalletContractPlan> walletContractPlans)
        {
            WalletContractPlans.AddRange(walletContractPlans);
        }

        public void SetStatus(WalletContractStatus status)
        {
            Status = status;
        }

        public void AddWalletContractRejectionReasons(string reason)
        {
            WalletContractRejectionReasons.Add(new WalletContractRejectionReason(reason));
        }

        public void SetGrantingProcessId(int grantingProcessId)
        {
            GrantingProcessId = grantingProcessId;
        }

        public void SetParentId(int parentId)
        {
            ParentId = parentId;
        }

        public void SetRootParentId(int? rootParentId)
        {
            RootParentId = rootParentId;
        }

        public void SetChangeStatusDate()
        {
            ChangeStatusDate = DateTime.Now;
        }

        public void SetEndDate(DateTime? endDate)
        {
            if (GrantingProcessId.HasValue)
            {
                EndDate = null;
            }
            else
            {
                EndDate = endDate;
            }
        }
    }
}
