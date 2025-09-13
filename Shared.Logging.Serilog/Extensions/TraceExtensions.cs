using Serilog.Context;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Shared.Logging.Serilog.Extensions;

public static class TraceExtensions
{
    public static void AddTraceId(this ILogger logging, string traceId)
    {
        LogContext.PushProperty("RequestId", traceId);
    }
}
