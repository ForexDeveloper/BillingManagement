namespace Application.Service.Helper;

public class CreditDetailsDto
{
    public decimal InstallmentAmount { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal LastInstallmentAmount { get; set; }
    public decimal LastInterestAmount { get; set; }
    public int NumberOfInstallments { get; set; }
    public decimal RepayableCreditAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal OperationalFeeAmount { get; set; }
}