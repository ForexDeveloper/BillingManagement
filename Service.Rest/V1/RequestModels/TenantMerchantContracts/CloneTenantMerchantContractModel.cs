namespace Service.Rest.V1.RequestModels.TenantMerchantContracts;


public class CloneTenantMerchantContractModel : CloneTenantMerchantContractBaseModel
{
    public int TenantId { get; set; }
}

public class CloneTenantMerchantContractBaseModel : CreateTenantMerchantContractBaseModel
{
}