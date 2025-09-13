using Domain.Core.Enums;
using Domain.Core.Helper;

namespace Application.Query.ViewModels.Providers;

public class GetProviderVm
{
    public int Id { get; set; }
    public ProviderType ProviderType { get; set; }
    public string ProviderTypeName { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string EnglishName { get; set; }

}