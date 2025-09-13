using Domain.Base;
using Domain.Core.Entities.BusinessEntity;

namespace Domain.Core.Entities.WalletContractAggregate;

public class WalletContractBusinessIdentity : BaseEntity<int>
{
    #region Property
    public int WalletContractId { get; private set; }
    public WalletContract WalletContract { get; private set; }
    public int BusinessIdentityId { get; private set; }
    public BusinessIdentity BusinessIdentity { get; private set; }
    public bool HasWallet { get; private set; }

    #endregion

    public WalletContractBusinessIdentity(int walletContractId, int businessIdentityId, bool hasWallet = false)
    {
        WalletContractId = walletContractId;
        BusinessIdentityId = businessIdentityId;
        HasWallet = hasWallet;
    }

    public void WalletCreated()
    {
        HasWallet = true;
    }
}