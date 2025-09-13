namespace Shared.Exception.Abstraction;

public sealed record ErrorModel(string Type)
{

    public IEnumerable<ErrorDetail> Details { get; set; } = new List<ErrorDetail>();
}

public sealed record ErrorDetail(string Key, string Description) { }

