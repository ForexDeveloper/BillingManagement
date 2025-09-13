using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.Transactions;

public class CreateCashTransactionModel
{
    public int CustomerId { get; set; }
    public int MerchantId { get; set; }
    public int TenantId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public int PaymentId { get; set; }
    public List<CreateCashPaymentModel> Payments { get; set; }
}

public class CreateCashPaymentModel
{
    public FinancialDocumentPaymentType Type { get; set; } = FinancialDocumentPaymentType.Credit;
    public decimal Amount { get; set; }
    public int? WalletId { get; set; }
    public int PaymentDetailId { get; set; }
}
