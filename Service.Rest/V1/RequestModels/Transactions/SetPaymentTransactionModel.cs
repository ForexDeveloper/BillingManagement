using Application.Service.Dtos.FinancialDocuments;

namespace Service.Rest.V1.RequestModels.Transactions;

public class SetPaymentTransactionModel
{
    public PaymentServiceType PaymentServiceType { get; set; }
    public int CustomerId { get; set; }
    public int TenantId { get; set; }
    public decimal Amount { get; set; }
    public int PaymentId { get; set; }
    public List<CreatePaymentModel> PaymentDetails { get; set; }
}
