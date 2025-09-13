namespace Service.Rest.V1.RequestModels.WalletContracts;

public class UpdateWalletContractModel : UpdateWalletContractBaseModel
{
    public int TenantId { get; set; }
}


public class UpdateWalletContractBaseModel : CreateWalletContractBaseModel
{
}