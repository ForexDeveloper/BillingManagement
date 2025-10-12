using System;
using Domain.Core.Entities.InstallmentAggregate.Dtos;
using Domain.Core.Entities.FinancialDocumentAggregate.Dtos;

namespace Application.Service.Dtos.Shared;

public sealed record FinancialDataRange
{
    public DateTime MinDate { get; set; }

    public DateTime MaxDate { get; set; }

    private FinancialDataRange(InstallmentRange installmentRange, FinancialDocumentRange financialDocumentRange)
    {
        if (installmentRange != null && financialDocumentRange != null)
        {
            MinDate = installmentRange.MinDueDate < financialDocumentRange.MinCreatedDateTime
                ? installmentRange.MinDueDate
                : financialDocumentRange.MinCreatedDateTime;

            MaxDate = installmentRange.MaxDueDate > financialDocumentRange.MaxCreatedDateTime
                ? installmentRange.MaxDueDate
                : financialDocumentRange.MaxCreatedDateTime;

            return;
        }

        if (installmentRange != null)
        {
            MinDate = installmentRange.MinDueDate;

            MaxDate = installmentRange.MaxDueDate;

            return;
        }

        if (financialDocumentRange != null)
        {
            MinDate = financialDocumentRange.MinCreatedDateTime;

            MaxDate = financialDocumentRange.MaxCreatedDateTime;
        }
    }

    public static FinancialDataRange? Create(InstallmentRange installmentRange, FinancialDocumentRange financialDocumentRange)
    {
        if (installmentRange == null && financialDocumentRange == null)
        {
            return null;
        }

        return new FinancialDataRange(installmentRange, financialDocumentRange);
    }
}