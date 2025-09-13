namespace Application.Service.Dtos.Wallet;

public class GetWalletPrePaymentDetails
{
    public decimal Balance { get; set; }
    public int CustomerId { get; set; }
    public decimal? PrepaymentPercent { get; set; }
    public decimal? PrepaymentMinAmount { get; set; }
    public decimal? PrepaymentMaxAmount { get; set; }
    public decimal CalculatedPrepaymentAmount { get; set; }
}