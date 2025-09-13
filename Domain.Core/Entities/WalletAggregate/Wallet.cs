using Domain.Base;
using Domain.Core.Entities.AccountAggregate;
using Domain.Core.Entities.BusinessEntity;
using Domain.Core.Entities.PlanAggregate;
using Domain.Core.Entities.TenantAggregate;
using Domain.Core.Entities.WalletContractAggregate;
using Domain.Core.Enums;
using System;

namespace Domain.Core.Entities.WalletAggregate
{
    public class Wallet : BaseEntity<int>
    {
        #region Property

        public WalletStatus Status { get; private set; }

        public int BusinessIdentityId { get; private set; }
        public BusinessIdentity BusinessIdentity { get; private set; }
        public int TenantId { get; private set; }
        public Tenant Tenant { get; private set; }
        public int AccountId { get; private set; }
        public Account Account { get; private set; }
        public int PlanId { get; private set; }
        public Plan Plan { get; private set; }
        public int WalletContractId { get; private set; }
        public WalletContract WalletContract { get; private set; }
        public bool IsDefault { get; private set; }

        #endregion Property

        protected Wallet() { }

        public Wallet(int businessIdentityId, int tenantId, int accountId, int planId, int walletContractId)
        {
            BusinessIdentityId = businessIdentityId;
            TenantId = tenantId;
            AccountId = accountId;
            PlanId = planId;
            WalletContractId = walletContractId;
            Status = WalletStatus.Active;
        }

        public Wallet(int businessIdentityId, int tenantId, Account account, int planId, int walletContractId)
        {
            BusinessIdentityId = businessIdentityId;
            TenantId = tenantId;
            Account = account;
            PlanId = planId;
            WalletContractId = walletContractId;
            Status = WalletStatus.Active;
        }

        public void SetDefault(bool isDefault)
        {
            IsDefault = isDefault;
        }

        public void SetStatus(WalletStatus status)
        {
            Status = status;
            SetEditDateTime(DateTime.Now);
        }
    }
}
