using Domain.Base;
using Domain.Core.Entities.PlanAggregate;

namespace Domain.Core.Entities.WalletContractAggregate;

public class WalletContractPlan : BaseEntity<int>
{
    #region Property
    public int WalletContractId { get; private set; }
    public WalletContract WalletContract { get; private set; }
    public int PlanId { get; private set; }
    public Plan Plan { get; private set; }

    public WalletContractPlan(int walletContractId, int planId)
    {
        WalletContractId = walletContractId;
        PlanId = planId;
    }

    public WalletContractPlan(WalletContract walletContract,Plan plan)
    {
        WalletContract = walletContract;
        Plan = plan;
    }

    #endregion
}