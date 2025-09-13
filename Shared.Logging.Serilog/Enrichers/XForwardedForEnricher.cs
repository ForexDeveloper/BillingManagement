using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Shared.Logging.Serilog.Enrichers;

class XForwardedForEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public XForwardedForEnricher() : this(new HttpContextAccessor())
    { }
    XForwardedForEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var xForwardedFor = _httpContextAccessor.HttpContext?.Request.Headers["x-forwarded-for"].ToString();
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("x-forwarded-for", xForwardedFor));
    }
}
