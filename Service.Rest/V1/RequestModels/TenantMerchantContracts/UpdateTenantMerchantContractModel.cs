namespace Service.Rest.V1.RequestModels.TenantMerchantContracts;

public class UpdateTenantMerchantContractModel : UpdateTenantMerchantContractBaseModel
{
    public int TenantId { get; set; }
}

public class UpdateTenantMerchantContractBaseModel : CreateTenantMerchantContractBaseModel
{
}
