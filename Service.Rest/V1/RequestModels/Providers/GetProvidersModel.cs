using Application.Query.Base;
using Domain.Core.Enums;

namespace Service.Rest.V1.RequestModels.Providers;

public class GetProvidersModel : BasePaginatedListRequest
{
    public ProviderType? ProviderType { get; set; }
}