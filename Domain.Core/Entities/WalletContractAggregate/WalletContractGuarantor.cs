using Domain.Base;
using System.Linq;
using Domain.Core.Enums;
using System.Collections.Generic;
using Domain.Core.Entities.Shared;
using Domain.Core.Entities.GuarantorAggregate;

namespace Domain.Core.Entities.WalletContractAggregate;

public class WalletContractGuarantor : BaseEntity<int>
{
    #region Property

    public int WalletContractId { get; private set; }

    public WalletContract WalletContract { get; private set; }

    public int GuarantorId { get; private set; }

    public Guarantor Guarantor { get; private set; }

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

    private WalletContractGuarantor() { }

    public WalletContractGuarantor(int id, int walletContractId, int guarantorId, List<byte> portionTypes,
        byte? commissionCalculationType,
        decimal? fixedAmountCommission, decimal? fixedPercentageCommission,
        decimal? transactionMinCommissionAmount, decimal? transactionMaxCommissionAmount,
        decimal? periodMinCommissionAmount, decimal? periodMaxCommissionAmount, byte? paymentMethodType)
    {
        Id = id;
        WalletContractId = walletContractId;
        GuarantorId = guarantorId;
        PortionTypes = portionTypes.Select(b => (WalletPortionType)b).ToList();
        CommissionCalculationType = commissionCalculationType != null ? (CommissionCalculationType)commissionCalculationType : null;
        FixedAmountCommission = fixedAmountCommission;
        FixedPercentageCommission = fixedPercentageCommission;
        TransactionMinCommissionAmount = transactionMinCommissionAmount;
        TransactionMaxCommissionAmount = transactionMaxCommissionAmount;
        PeriodMinCommissionAmount = periodMinCommissionAmount;
        PeriodMaxCommissionAmount = periodMaxCommissionAmount;
        PaymentMethodType = paymentMethodType != null ? (PaymentMethodType)paymentMethodType : null;
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