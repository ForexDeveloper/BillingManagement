using Application.Query.Base;
using Domain.Core.Enums;
using System;

namespace Application.Query.QueryModels;

public class CustomerCashOutRequestQueryModel
{
    public long Id { get; set; } 
    public string Iban { get; set; }
    public decimal Amount { get; set; }
    public string FullName { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public string NationalCode { get; set; }
    public string Mobile { get; set; }
    public string BankTransactionCode { get; set; }
    public CashOutRequestStatus Status { get; set; }
    public CashOutRequestRejectReason? RejectReason { get; set; }
    public long FollowUpCode { get; set; }
    public string Description { get; set; }
    public int CustomerId { get; set; }
}

public class CustomerCashOutRequestsQueryModel : BasePaginatedListQueryResult<CustomerCashOutRequestQueryModel>
{

}