namespace Application.Service.Dtos.CashWallet;

public class CreateCashOutRequestDto
{
    public int BusinessIdentityId { get; set; }
    public int  BankAccountId { get; set; }
    public decimal Amount { get; set; }
    public int  TenantId { get; set; }
}