using System;

namespace Domain.Core.Entities.InstallmentAggregate.Dtos;

public sealed record InstallmentDto
{
    public decimal Amount { get;  set; }

    public decimal CashAmount { get;  set; }

    public decimal CreditAmount { get;  set; }

    public decimal PrepaymentAmount { get;  set; }

    public decimal Commission { get;  set; }
    
    public DateTime DueDate { get; set; }

    public InstallmentDto()
    {
        
    }

    public InstallmentDto(decimal amount, decimal cashAmount, decimal creditAmount, decimal prepaymentAmount, decimal commission, DateTime dueDate)
    {
        Amount = amount;
        DueDate = dueDate;
        CashAmount = cashAmount;
        Commission = commission;
        CreditAmount = creditAmount;
        PrepaymentAmount = prepaymentAmount;
    }
}