namespace Service.Rest.V1.RequestModels.Billings;

public class CreateSettlementChequesBillingPaymentRequest
{
    public int TenantId { get; set; }
    public long PaymentId { get; set; }
}