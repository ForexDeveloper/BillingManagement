using System;
using Domain.Core.Enums;

namespace Domain.Core.Entities.FinancialDocumentAggregate.Dtos;

public sealed record FinancialDocumentDto
{
    public decimal Amount { get; set; }

    public decimal? PurchaseCommission { get; set; }

    public DateTime CreatedDateTime { get; set; }
}