namespace Application.Service.Dtos.CashWallet
{
    public class CreateCashWalletWithAddCustomerEventDto
    {
        public int TenantId { get; set; }
        public int CustomerId { get; set; }
    }
}
