using Domain.Core.Entities.Shared;
using Domain.Core.Enums;
using System.Collections.Generic;

namespace Application.Query.QueryModels;

public class WalletContractFacilitatorQueryModel
{
    public int Id { get; set; }
    public int WalletContractId { get; set; }
    public int FacilitatorId { get; set; }
    public string FacilitatorTitle { get; set; }
    public List<WalletPortionType> PortionTypes { get; set; }
    public decimal Percentage { get; set; }
    public CommissionCalculationType? CommissionCalculationType { get; set; }
    public string CommissionCalculationTypeTitle { get; set; }
    public decimal? FixedAmountCommission { get; set; }
    public decimal? FixedPercentageCommission { get; set; }
    public decimal? TransactionMinCommissionAmount { get; set; }
    public decimal? TransactionMaxCommissionAmount { get; set; }
    public decimal? PeriodMinCommissionAmount { get; set; }
    public decimal? PeriodMaxCommissionAmount { get; set; }
    public List<TieredCommission> TieredCommissions { get; set; } = [];
    public PaymentMethodType? PaymentMethodType { get; set; }

    public WalletContractFacilitatorQueryModel(int id, int walletContractId, int facilitatorId, string facilitatorTitle, List<WalletPortionType> portionTypes, decimal percentage)
    {
        Id = id;
        WalletContractId = walletContractId;
        FacilitatorId = facilitatorId;
        FacilitatorTitle = facilitatorTitle;
        PortionTypes = portionTypes;
        Percentage = percentage;
    }

    public WalletContractFacilitatorQueryModel(int id, int walletContractId, int facilitatorId, string facilitatorTitle, List<WalletPortionType> portionTypes,
        CommissionCalculationType? commissionCalculationType, decimal? fixedAmountCommission, decimal? fixedPercentageCommission,
        decimal? transactionMinCommissionAmount, decimal? transactionMaxCommissionAmount, decimal? periodMinCommissionAmount, decimal? periodMaxCommissionAmount,
        List<TieredCommission> tieredCommissions, PaymentMethodType? paymentMethodType)
    {
        Id = id;
        WalletContractId = walletContractId;
        FacilitatorId = facilitatorId;
        FacilitatorTitle = facilitatorTitle;
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