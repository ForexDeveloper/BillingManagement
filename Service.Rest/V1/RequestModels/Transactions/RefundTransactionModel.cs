using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.Transactions;

public class RefundTransactionModel
{
    public long FinancialDocumentId { get; set; }
    public decimal Amount { get; set; }
    public RefundReason Reason { get; set; }
    public string Description { get; set; }
}
