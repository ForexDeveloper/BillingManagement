using System;

namespace Domain.Core.Entities.FinancialDocumentAggregate.Dtos;

public sealed record FinancialDocumentRange(DateTime MinCreatedDateTime, DateTime MaxCreatedDateTime)
{
    public DateTime MinCreatedDateTime { get; set; } = MinCreatedDateTime.Date;

    public DateTime MaxCreatedDateTime { get; set; } = MaxCreatedDateTime.Date;
}