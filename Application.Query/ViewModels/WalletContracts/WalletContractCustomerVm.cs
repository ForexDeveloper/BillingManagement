namespace Application.Query.ViewModels.WalletContracts;

public class WalletContractCustomerVm
{
    public int CustomerId { get; private set; }
    public string CustomerName { get; private set; }

    public WalletContractCustomerVm(int customerId, string customerName)
    {
        CustomerId = customerId;
        CustomerName = customerName;
    }
}