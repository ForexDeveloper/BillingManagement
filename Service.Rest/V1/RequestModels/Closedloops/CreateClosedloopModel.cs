using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.Closedloops
{
    public class CreateClosedloopModel : CreateClosedloopBaseModel
    {
        public int TenantId { get; set; }

    }
    public class CreateClosedloopBaseModel
    {
        public int WalletConfigurationId { get; set; }
        public string Title { get; set; }
        public List<int> Categories { get; set; }
        public List<int> Merchants { get; set; }

    }
}