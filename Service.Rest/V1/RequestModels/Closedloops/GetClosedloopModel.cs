using Application.Command.WalletConfigurationCommands;
using Application.Query.Base;
using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.Closedloops
{
    public class GetClosedloopModel : GetClosedloopBaseModel
    {
        public int TenantId { get; set; }
    }
    public class GetClosedloopBaseModel : BasePaginatedListRequest
    {
        public int WalletConfigurationId { get; set; }
    }

}