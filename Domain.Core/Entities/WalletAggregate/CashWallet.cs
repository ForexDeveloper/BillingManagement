using Domain.Core.Entities.AccountAggregate;

namespace Domain.Core.Entities.WalletAggregate
{
    public class CashWallet : Wallet
    {
        private CashWallet() { }

        public CashWallet(int businessIdentityId, int tenantId, int accountId, int planId, int walletContractId)
            : base(businessIdentityId, tenantId, accountId, planId, walletContractId)
        {
        }

        public CashWallet(int businessIdentityId, int tenantId, Account account, int planId, int walletContractId)
          : base(businessIdentityId, tenantId, account, planId, walletContractId)
        {
        }

    }
}
