using Domain.Base;
using Domain.Core.Entities.FinancierAggregate;
using Domain.Core.Entities.Shared;
using Domain.Core.Enums;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Core.Entities.WalletContractAggregate;

public class WalletContractFinancier : BaseEntity<int>
{
    #region Property
    public int WalletContractId { get; private set; }
    public WalletContract WalletContract { get; private set; }
    public int FinancierId { get; private set; }
    public Financier Financier { get; private set; }
    public List<WalletPortionType> PortionTypes { get; private set; }
    public BmCommissionCalculationType? CommissionCalculationType { get; private set; }
    public decimal? FixedAmountCommission { get; private set; }
    public decimal? FixedPercentageCommission { get; private set; }
    public decimal? TransactionMinCommissionAmount { get; private set; }
    public decimal? TransactionMaxCommissionAmount { get; private set; }
    public decimal? PeriodMinCommissionAmount { get; private set; }
    public decimal? PeriodMaxCommissionAmount { get; private set; }
    public List<TieredCommission>? TieredCommissions { get; private set; } = [];
    public BmPaymentMethodType? PaymentMethodType { get; private set; }

    private WalletContractFinancier() { }

    public WalletContractFinancier(int id, int walletContractId, int financierId, List<byte> portionTypes,
        byte? commissionCalculationType,
        decimal? fixedAmountCommission, decimal? fixedPercentageCommission,
        decimal? transactionMinCommissionAmount, decimal? transactionMaxCommissionAmount,
        decimal? periodMinCommissionAmount, decimal? periodMaxCommissionAmount, byte? paymentMethodType)
    {
        Id = id;
        WalletContractId = walletContractId;
        FinancierId = financierId;
        PortionTypes = portionTypes.Select(b => (WalletPortionType)b).ToList();
        CommissionCalculationType = (BmCommissionCalculationType)commissionCalculationType;
        FixedAmountCommission = fixedAmountCommission;
        FixedPercentageCommission = fixedPercentageCommission;
        TransactionMinCommissionAmount = transactionMinCommissionAmount;
        TransactionMaxCommissionAmount = transactionMaxCommissionAmount;
        PeriodMinCommissionAmount = periodMinCommissionAmount;
        PeriodMaxCommissionAmount = periodMaxCommissionAmount;
        PaymentMethodType = (BmPaymentMethodType)paymentMethodType;
    }

    public void SetTieredCommissions(List<TieredCommission> tieredCommissions)
    {
        if (CommissionCalculationType == null)
            return;

        TieredCommission.ValidateInputList(tieredCommissions);

        if (tieredCommissions != null &&
            (CommissionCalculationType == Enums.BmCommissionCalculationType.UniformTiered ||
            CommissionCalculationType == Enums.BmCommissionCalculationType.CumulativeTiered))
        {
            TieredCommissions = tieredCommissions;
        }
    }

    public void Update(List<WalletPortionType> portionTypes,
        BmCommissionCalculationType? commissionCalculationType,
        decimal? fixedAmountCommission, decimal? fixedPercentageCommission,
        decimal? transactionMinCommissionAmount, decimal? transactionMaxCommissionAmount,
        decimal? periodMinCommissionAmount, decimal? periodMaxCommissionAmount, BmPaymentMethodType? paymentMethodType)
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