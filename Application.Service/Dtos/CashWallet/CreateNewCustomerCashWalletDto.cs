namespace Application.Service.Dtos.CashWallet;

public class CreateNewCustomerCashWalletDto
{
    public int PlanId { get; set; }
    public int CustomerId { get; set; }
    public int TenantId { get; set; }
    public int WalletContractId { get; set; }
}