using Domain.Base;
using Domain.Core.Entities.FinancierAggregate;
using Domain.Core.Entities.Shared;
using Domain.Core.Enums;
using System.Collections.Generic;

namespace Domain.Core.Entities.WalletContractAggregate;

public class WalletContractFinancier : BaseEntity<int>
{
    #region Property
    public int WalletContractId { get; private set; }
    public WalletContract WalletContract { get; private set; }
    public int FinancierId { get; private set; }
    public Financier Financier { get; private set; }
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

    public WalletContractFinancier(int walletContractId, int financierId, List<WalletPortionType> portionTypes)
    {
        WalletContractId = walletContractId;
        FinancierId = financierId;
        PortionTypes = portionTypes;
    }

    public WalletContractFinancier(int walletContractId, int financierId, List<WalletPortionType> portionTypes,
        CommissionCalculationType? commissionCalculationType,
        decimal? fixedAmountCommission, decimal? fixedPercentageCommission,
        decimal? transactionMinCommissionAmount, decimal? transactionMaxCommissionAmount,
        decimal? periodMinCommissionAmount, decimal? periodMaxCommissionAmount, PaymentMethodType? paymentMethodType)
    {
        WalletContractId = walletContractId;
        FinancierId = financierId;
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