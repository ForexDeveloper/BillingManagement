namespace Application.Service.Dtos.Shared;

public class TieredCommissionDto
{
    public decimal FromAmount { get; set; }
    public decimal? ToAmount { get; set; }
    public decimal Percentage { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }

    public TieredCommissionDto()
    {
    }

    public TieredCommissionDto(decimal fromAmount, decimal? toAmount, decimal percentage, decimal? minAmount, decimal? maxAmount)
    {
        FromAmount = fromAmount;
        ToAmount = toAmount;
        Percentage = percentage;
        MinAmount = minAmount;
        MaxAmount = maxAmount;
    }
}
