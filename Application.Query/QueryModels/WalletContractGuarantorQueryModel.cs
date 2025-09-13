using Domain.Core.Entities.Shared;
using Domain.Core.Enums;
using System.Collections.Generic;

namespace Application.Query.QueryModels;

public class WalletContractGuarantorQueryModel
{
    public int Id { get; set; }
    public int WalletContractId { get; set; }
    public int GuarantorId { get; set; }
    public string GuarantorTitle { get; set; }
    public List<WalletPortionType> PortionTypes { get; set; }
    public decimal Percentage { get; set; }
    public CommissionCalculationType? CommissionCalculationType { get; set; }
    public decimal? FixedAmountCommission { get; set; }
    public decimal? FixedPercentageCommission { get; set; }
    public decimal? TransactionMinCommissionAmount { get; set; }
    public decimal? TransactionMaxCommissionAmount { get; set; }
    public decimal? PeriodMinCommissionAmount { get; set; }
    public decimal? PeriodMaxCommissionAmount { get; set; }
    public List<TieredCommission> TieredCommissions { get; set; } = [];
    public PaymentMethodType? PaymentMethodType { get; set; }

    public WalletContractGuarantorQueryModel(int id, int walletContractId, int guarantorId, string guarantorTitle, List<WalletPortionType> portionTypes, decimal percentage)
    {
        Id = id;
        WalletContractId = walletContractId;
        GuarantorId = guarantorId;
        GuarantorTitle = guarantorTitle;
        PortionTypes = portionTypes;
        Percentage = percentage;
    }

    public WalletContractGuarantorQueryModel(int id, int walletContractId, int guarantorId, string guarantorTitle, List<WalletPortionType> portionTypes,
        CommissionCalculationType? commissionCalculationType, decimal? fixedAmountCommission, decimal? fixedPercentageCommission,
        decimal? transactionMinCommissionAmount, decimal? transactionMaxCommissionAmount, decimal? periodMinCommissionAmount, decimal? periodMaxCommissionAmount,
        List<TieredCommission> tieredCommissions, PaymentMethodType? paymentMethodType)
    {
        Id = id;
        WalletContractId = walletContractId;
        GuarantorId = guarantorId;
        GuarantorTitle = guarantorTitle;
        PortionTypes = portionTypes;
        CommissionCalculationType = commissionCalculationType;
        FixedAmountCommission = fixedAmountCommission;
        FixedPercentageCommission = fixedPercentageCommission;
        TransactionMinCommissionAmount = transactionMinCommissionAmount;
        TransactionMaxCommissionAmount = transactionMaxCommissionAmount;
        PeriodMinCommissionAmount = periodMinCommissionAmount;
        PeriodMaxCommissionAmount = periodMaxCommissionAmount;
        TieredCommissions = tieredCommissions;
        PaymentMethodType = paymentMethodType;
    }
}