using System;
using Domain.Core.Enums;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;

namespace Domain.Core.Entities.BillingAggregate.Dtos;

public sealed record BillingDto
{
    public BillingType Type { get; set; }

    public bool CurrentPeriod { get; set; }

    public DateTime StartOfPeriod { get; set; }

    public DateTime EndOfPeriod { get; set; }

    public ContractGroup ContractGroup { get; set; }
}