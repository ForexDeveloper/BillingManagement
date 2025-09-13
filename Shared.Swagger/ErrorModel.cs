namespace Shared.Swagger;

public sealed record ErrorModel(ushort Code, string Message, string Type)
{

    public IEnumerable<ErrorDetail> Details { get; set; } = new List<ErrorDetail>();
}

public sealed record ErrorDetail(string Key, string Description) { }

