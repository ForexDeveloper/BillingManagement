using Domain.Core.Enums;
using System;
using System.Collections.Generic;

namespace Application.Query.ViewModels.FinancialDocuments;

public class RefundFinancialDocumentViewModel
{
    public long FinancialDocumentId { get; set; }
    public decimal Amount { get; set; }
    public decimal RemainAmount { get; set; }
    public List<RefundFinancialDocumentDetailViewModel> RefundDetails { get; set; }
}

public class RefundFinancialDocumentDetailViewModel
{
    public DateTime RefundDateTime { get; set; }
    public decimal RefundAmount { get; set; }
    public decimal RemainAmount { get; set; }
    public RefundReason RefundReason { get; set; }
    public string RefundReasonTitle { get; set; }
    public string RefundDescription { get; set; }
}