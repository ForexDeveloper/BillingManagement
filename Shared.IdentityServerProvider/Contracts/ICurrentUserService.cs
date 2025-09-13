namespace Shared.IdentityServerProvider.Contracts;

public interface ICurrentUserService
{
    public string? UserId { get; }
    public string? ClientId { get; }
    public int TenantId { get; }
    public bool HasClaim(string claim);
}
