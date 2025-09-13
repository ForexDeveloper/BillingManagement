using Application.Service.Dtos.FinancialDocuments;

namespace Service.Rest.V1.RequestModels.Billings;

public class CreateBillingPaymentRequest
{
    public long PaymentId { get; set; }
    public int TenantId { get; set; }
    public decimal Amount { get; set; }
    public List<PaymentDetailDto> PaymentDetails { get; set; }
}