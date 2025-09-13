using Application.Service.Dtos.Shared;
using Domain.Core.Enums;
using System.Collections.Generic;

namespace Application.Query.ViewModels.WalletContracts;

public class WalletContractFacilitatorVm
{
    public int FacilitatorId { get; set; }
    public string FacilitatorTitle { get; set; }
    public List<WalletPortionType> PortionTypes { get; set; }
    public List<string> PortionTypeTitles { get; set; }
    public CommissionCalculationType? CommissionCalculationType { get; set; }
    public string CommissionCalculationTypeTitle { get; set; }
    public decimal? FixedAmountCommission { get; set; }
    public decimal? FixedPercentageCommission { get; set; }
    public decimal? TransactionMinCommissionAmount { get; set; }
    public decimal? TransactionMaxCommissionAmount { get; set; }
    public decimal? PeriodMinCommissionAmount { get; set; }
    public decimal? PeriodMaxCommissionAmount { get; set; }
    public List<TieredCommissionDto>? TieredCommissions { get; set; } = [];
    public PaymentMethodType? PaymentMethodType { get; set; }
    public string PaymentMethodTypeTitle { get; set; }
}