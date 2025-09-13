using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Shared.Logging.Serilog.Enrichers;

class IdentityUserEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityUserEnricher() : this(new HttpContextAccessor())
    {
    }

    IdentityUserEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        string client = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(f => f.Type == "client_id")?.Value ?? "anonymous";
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("ClientId", client));

        string userName = _httpContextAccessor.HttpContext?.User?.Claims
                              .FirstOrDefault(f => f.Type == ClaimTypes.Name)?.Value ??
                          "anonymous";
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("UserName", userName));
    }
}