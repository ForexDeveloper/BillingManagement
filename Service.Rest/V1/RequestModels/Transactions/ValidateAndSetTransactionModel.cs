using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.Transactions;

public class ValidateAndSetTransactionModel
{
    public int CustomerId { get; set; }
    public int MerchantId { get; set; }
    public int? MerchantBranchId { get; set; }
    public int TenantId { get; set; }
    public decimal Amount { get; set; }
    public int PaymentId { get; set; }
    public PurchaseGatewayType PurchaseGatewayType { get; set; }
    public List<CreatePaymentModel> Payments { get; set; }
}

public class CreatePaymentModel
{
    public FinancialDocumentPaymentType Type { get; set; }
    public decimal Amount { get; set; }
    public int? WalletId { get; set; }
    public int PaymentDetailId { get; set; }
}
