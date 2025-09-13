namespace Application.Service.Dtos.Shared;

public class PublicAppConfiguration
{
    public string ApplicationName { get; set; }
    public int PlatformTenantId { get; set; }
    public string EncryptionKey { get; set; }
}
