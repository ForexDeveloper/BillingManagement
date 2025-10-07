using System;

namespace Domain.Core.Entities.InstallmentAggregate.Dtos;

public sealed record InstallmentRange(DateTime MinDueDate, DateTime MaxDueDate)
{
    public DateTime MinDueDate { get; set; } = MinDueDate;

    public DateTime MaxDueDate { get; set; } = MaxDueDate;
}