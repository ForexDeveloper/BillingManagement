using Domain.Core.Enums;

namespace Application.Service.Dtos.FinancialDocuments;

public class PaymentDetailDto
{
    public FinancialDocumentPaymentType Type { get; set; }
    public decimal Amount { get; set; }
    public long PaymentDetailId { get; set; }
    public int? WalletId { get; set; }
    public int? WalletContractId { get; set; }
}
