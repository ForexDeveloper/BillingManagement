using Domain.Core.Enums;

namespace Application.Query.QueryModels;

public class ProviderQueryModel
{
    public int Id { get; set; }
    public ProviderType ProviderType { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string EnglishName { get; set; }

}