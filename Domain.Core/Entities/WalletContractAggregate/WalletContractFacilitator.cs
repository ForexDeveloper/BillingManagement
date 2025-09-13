using Domain.Base;
using Domain.Core.Entities.FacilitatorAggregate;
using Domain.Core.Entities.Shared;
using Domain.Core.Enums;
using System.Collections.Generic;

namespace Domain.Core.Entities.WalletContractAggregate;

public class WalletContractFacilitator : BaseEntity<int>
{
    #region Property
    public int WalletContractId { get; private set; }
    public WalletContract WalletContract { get; private set; }
    public int FacilitatorId { get; private set; }
    public Facilitator Facilitator { get; private set; }
    public List<WalletPortionType> PortionTypes { get; private set; }

    public CommissionCalculationType? CommissionCalculationType { get; private set; }
    public decimal? FixedAmountCommission { get; private set; }
    public decimal? FixedPercentageCommission { get; private set; }
    public decimal? TransactionMinCommissionAmount { get; private set; }
    public decimal? TransactionMaxCommissionAmount { get; private set; }
    public decimal? PeriodMinCommissionAmount { get; private set; }
    public decimal? PeriodMaxCommissionAmount { get; private set; }
    public List<TieredCommission>? TieredCommissions { get; private set; } = [];
    public PaymentMethodType? PaymentMethodType { get; private set; }

    public WalletContractFacilitator(int walletContractId, int facilitatorId, List<WalletPortionType> portionTypes)
    {
        WalletContractId = walletContractId;
        FacilitatorId = facilitatorId;
        PortionTypes = portionTypes;
    }

    public WalletContractFacilitator(int walletContractId, int facilitatorId, List<WalletPortionType> portionTypes,
        CommissionCalculationType? commissionCalculationType,
        decimal? fixedAmountCommission, decimal? fixedPercentageCommission,
        decimal? transactionMinCommissionAmount, decimal? transactionMaxCommissionAmount,
        decimal? periodMinCommissionAmount, decimal? periodMaxCommissionAmount, PaymentMethodType? paymentMethodType)
    {
        WalletContractId = walletContractId;
        FacilitatorId = facilitatorId;
        PortionTypes = portionTypes;
        CommissionCalculationType = commissionCalculationType;
        FixedAmountCommission = fixedAmountCommission;
        FixedPercentageCommission = fixedPercentageCommission;
        TransactionMinCommissionAmount = transactionMinCommissionAmount;
        TransactionMaxCommissionAmount = transactionMaxCommissionAmount;
        PeriodMinCommissionAmount = periodMinCommissionAmount;
        PeriodMaxCommissionAmount = periodMaxCommissionAmount;
        PaymentMethodType = paymentMethodType;
    }

    public void SetTieredCommissions(List<TieredCommission> tieredCommissions)
    {
        if (CommissionCalculationType == null)
            return;

        TieredCommission.ValidateInputList(tieredCommissions);

        if (tieredCommissions != null &&
            (CommissionCalculationType == Enums.CommissionCalculationType.UniformTiered ||
            CommissionCalculationType == Enums.CommissionCalculationType.CumulativeTiered))
        {
            TieredCommissions = tieredCommissions;
        }
    }

    public void Update(List<WalletPortionType> portionTypes,
        CommissionCalculationType? commissionCalculationType,
        decimal? fixedAmountCommission, decimal? fixedPercentageCommission,
        decimal? transactionMinCommissionAmount, decimal? transactionMaxCommissionAmount,
        decimal? periodMinCommissionAmount, decimal? periodMaxCommissionAmount, PaymentMethodType? paymentMethodType)
    {
        PortionTypes = portionTypes;
        CommissionCalculationType = commissionCalculationType;
        FixedAmountCommission = fixedAmountCommission;
        FixedPercentageCommission = fixedPercentageCommission;
        TransactionMinCommissionAmount = transactionMinCommissionAmount;
        TransactionMaxCommissionAmount = transactionMaxCommissionAmount;
        PeriodMinCommissionAmount = periodMinCommissionAmount;
        PeriodMaxCommissionAmount = periodMaxCommissionAmount;
        PaymentMethodType = paymentMethodType;
    }

    public void ClearTieredCommissions()
    {
        TieredCommissions = [];
    }

    #endregion
}