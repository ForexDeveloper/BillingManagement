namespace Application.Service.Dtos.Wallet;
public class PrePaymentAmountDto
{
    public decimal? PrepaymentPercent { get; set; }
    public decimal? PrepaymentMinAmount { get; set; }
    public decimal? PrepaymentMaxAmount { get; set; }
}