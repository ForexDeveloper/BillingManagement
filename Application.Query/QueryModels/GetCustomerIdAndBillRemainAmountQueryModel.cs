using Domain.Core.Entities.BillingAggregate;
using Domain.Core.Enums;
using System;
using System.Collections.Generic;

namespace Application.Query.QueryModels;
public class GetCustomerIdAndBillRemainAmountQueryModel
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int FromAccountId { get; set; }
    public int CustomerId { get; set; }
    public List<BillingPayment> BillingPayments { get; set; }
    public decimal Amount { get; set; }
    public decimal PreviousDebitAmount { get; set; }
    public decimal PreviousCreditAmount { get; set; }
    public decimal PreviousPenaltyAmount { get; set; }
    public BillingState State { get; set; }
}
