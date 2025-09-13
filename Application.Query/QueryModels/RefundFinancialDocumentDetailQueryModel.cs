using Domain.Core.Enums;
using System;
using System.Collections.Generic;

namespace Application.Query.QueryModels;

public class RefundFinancialDocumentQueryModel
{
    public long FinancialDocumentId { get; set; }
    public decimal Amount { get; set; }
    public decimal RemainAmount { get; set; }
    public List<RefundFinancialDocumentDetailQueryModel> RefundDetails { get; set; } = [];
}

public class RefundFinancialDocumentDetailQueryModel
{
    public long RefundFinancialDocumentId { get; set; }
    public DateTime RefundDateTime { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal RemainAmount { get; set; }
    public RefundReason RefundReason { get; set; }
    public string RefundDescription { get; set; }
}
