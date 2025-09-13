namespace Shared.Swagger;

public record Configurations
{

    public string Title { get; set; } = "My Api";
    public Uri IdpServer { get; set; }
}