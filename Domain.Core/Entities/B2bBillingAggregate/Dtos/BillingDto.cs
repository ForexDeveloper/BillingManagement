using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;
using System;
using System.Collections.Generic;

namespace Domain.Core.Entities.B2bBillingAggregate.Dtos;

public sealed record BillingDto
{
    public required DateTime StartOfPeriod { get; set; }

    public required DateTime EndOfPeriod { get; set; }

    public required IEnumerable<int> ContractIds { get; set; }

    public required ContractGroup ContractGroup { get; set; }
}