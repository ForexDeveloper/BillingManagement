using Microsoft.AspNetCore.Http;
using Shared.IdentityServerProvider.Contracts;
using System.Security.Claims;

namespace Shared.IdentityServerProvider.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _contextAccessor;

    public CurrentUserService(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public string? UserId => _contextAccessor.HttpContext?.Request.Headers["user_id"] ?? "anonymous";

    public string? ClientId => _contextAccessor.HttpContext?.User?.FindFirstValue("client_id") ?? "anonymous";

    public int TenantId => _contextAccessor.HttpContext?.User?.FindFirstValue("tenant_id") != null ?
                int.Parse(_contextAccessor.HttpContext?.User?.FindFirstValue("tenant_id")) :
                int.Parse(_contextAccessor.HttpContext?.Request.Headers["tenant_id"]);

    public bool HasClaim(string claim)
    {
        var result = _contextAccessor.HttpContext?.User?.HasClaim(x => x.Type == "permission" && x.Value == claim);
        return result ?? false;
    }
}
