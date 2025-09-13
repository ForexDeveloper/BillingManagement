using Application.Service.Dtos.Shared;
using Domain.Core.Enums;
using System.Collections.Generic;

namespace Application.Service.Dtos.WalletContract;

public class WalletContractFinancierDto
{
    public int FinancierId { get; set; }
    public List<WalletPortionType>? PortionTypes { get; set; }
    public CommissionCalculationType? CommissionCalculationType { get; set; }
    public List<TieredCommissionDto> TieredCommissions { get; set; }
    public decimal? FixedAmountCommission { get; set; }
    public decimal? FixedPercentageCommission { get; set; }
    public decimal? TransactionMinCommissionAmount { get; set; }
    public decimal? TransactionMaxCommissionAmount { get; set; }
    public decimal? PeriodMinCommissionAmount { get; set; }
    public decimal? PeriodMaxCommissionAmount { get; set; }
    public PaymentMethodType? PaymentMethodType { get; set; }
}