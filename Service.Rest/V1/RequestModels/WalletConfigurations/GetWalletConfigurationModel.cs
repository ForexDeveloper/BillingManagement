using Application.Query.Base;

namespace Service.Rest.V1.RequestModels.WalletConfigurations
{
    public class GetWalletConfigurationModel : BasePaginatedListRequest
    {
        public int TenantId { get; set; }
    }
    public class GetWalletConfigurationBaseModel : BasePaginatedListRequest
    {
    }

}