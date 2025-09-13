using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.Purchases;

//currency get from payment or not ?
public class CreatePurchaseModel
{
    public int CustomerId { get; set; } // get UserId then CustomerId from token
    public int MerchantId { get; set; }
    public int TenantId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public int PaymentId { get; set; }
    public List<CreatePurchasePaymentModel> Payments { get; set; }
}

public class CreatePurchasePaymentModel
{
    public FinancialDocumentPaymentType Type { get; set; }
    public decimal Amount { get; set; }
    public int? WalletId { get; set; } // create cash wallet or not in background (when create cash wallet), need more discussion ?
    public int PaymentDetailId { get; set; }
}
