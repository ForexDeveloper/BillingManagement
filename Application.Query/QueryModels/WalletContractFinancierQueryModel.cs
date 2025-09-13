using Domain.Core.Entities.Shared;
using Domain.Core.Enums;
using System.Collections.Generic;

namespace Application.Query.QueryModels;

public class WalletContractFinancierQueryModel
{
    public int Id { get; set; }
    public int WalletContractId { get; set; }
    public int FinancierId { get; set; }
    public string FinancierTitle { get; set; }
    public List<WalletPortionType> PortionTypes { get; set; }
    public decimal Percentage { get; set; }
    public CommissionCalculationType? CommissionCalculationType { get; set; }
    public decimal? FixedAmountCommission { get; set; }
    public decimal? FixedPercentageCommission { get; set; }
    public decimal? TransactionMinCommissionAmount { get; set; }
    public decimal? TransactionMaxCommissionAmount { get; set; }
    public decimal? PeriodMinCommissionAmount { get; set; }
    public decimal? PeriodMaxCommissionAmount { get; set; }
    public List<TieredCommission>? TieredCommissions { get; set; } = [];
    public PaymentMethodType? PaymentMethodType { get; set; }

    public WalletContractFinancierQueryModel(int id, int walletContractId, int financierId, string financierTitle, List<WalletPortionType> portionTypes, decimal percentage)
    {
        Id = id;
        WalletContractId = walletContractId;
        FinancierId = financierId;
        FinancierTitle = financierTitle;
        PortionTypes = portionTypes;
        Percentage = percentage;
    }

    public WalletContractFinancierQueryModel(int id, int walletContractId, int financierId, string financierTitle, List<WalletPortionType> portionTypes,
        CommissionCalculationType? commissionCalculationType, decimal? fixedAmountCommission, decimal? fixedPercentageCommission,
        decimal? transactionMinCommissionAmount, decimal? transactionMaxCommissionAmount, decimal? periodMinCommissionAmount, decimal? periodMaxCommissionAmount,
        List<TieredCommission> tieredCommissions, PaymentMethodType? paymentMethodType)
    {
        Id = id;
        WalletContractId = walletContractId;
        FinancierId = financierId;
        FinancierTitle = financierTitle;
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