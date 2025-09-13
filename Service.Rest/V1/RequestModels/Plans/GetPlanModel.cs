using Application.Query.Base;

namespace Service.Rest.V1.RequestModels.Plans
{
    public class GetPlanModel : GetPlanBaseModel
    {
        public int TenantId { get; set; }
    }
    public class GetPlanBaseModel : BasePaginatedListRequest
    {
        public int WalletConfigurationId { get; set; }
    }


    public class GetPlanSimpleListModel
    {
        public int TenantId { get; set; }
    }
}