using System;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;

namespace Domain.Core.Entities.BillingAggregate.Dtos;

public sealed record BillingDto
{
    public required bool CurrentPeriod { get; set; }

    public required DateTime StartOfPeriod { get; set; }

    public required DateTime EndOfPeriod { get; set; }

    public required ContractGroup ContractGroup { get; set; }
}