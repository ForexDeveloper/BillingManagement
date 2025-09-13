namespace Domain.Core.Compositions;

public class BillingsPaymentsAmount
{
    public long BillId { get; set; }
    public long InstallmentId { get; set; }
    public decimal TotalAmount { get; set; }
}