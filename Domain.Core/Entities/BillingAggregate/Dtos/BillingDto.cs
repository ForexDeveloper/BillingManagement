using System;
using System.Collections.Generic;
using Domain.Core.Entities.InstallmentAggregate.Dtos;
using Domain.Core.Entities.FinancialDocumentAggregate.Dtos;
using Domain.Core.Entities.TenantMerchantContractAggregate.Dtos;

namespace Domain.Core.Entities.BillingAggregate.Dtos;

public sealed record BillingDto
{
    public required bool CurrentPeriod { get; set; }

    public required DateTime StartOfPeriod { get; set; }

    public required DateTime EndOfPeriod { get; set; }

    public required List<int> ContractIds { get; set; }

    public required ContractGroup ContractGroup { get; set; }

    public IEnumerable<InstallmentDto> Installments { get; set; }

    public IEnumerable<FinancialDocumentDto> FinancialDocuments { get; set; }
}