namespace Application.Service.Dtos.BillingPayments;

public class SettlementChequePaymentDto
{
    public long InstallmentId { get; set; }
    public long PaymentId { get; set; }
    public int TenantId { get; set; }
    public int CustomerId { get; set; }
}
