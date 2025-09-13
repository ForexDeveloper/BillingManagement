using Shared.Logging.Abstraction.Models;
using Microsoft.Extensions.Logging;

namespace Shared.Logging.Abstraction.Extensions;

public static class LoggingExtensions
{

    static Action<ILogger, string, Exception?> _traceLoggerMessage = LoggerMessage.Define<string>(LogLevel.Trace, eventId: new EventId(id: 0, name: "TRACE"), formatString: "{jsonMessage}");
    static Action<ILogger, string, Exception?> _infoLoggerMessage = LoggerMessage.Define<string>(LogLevel.Information, eventId: new EventId(id: 0, name: "Information"), formatString: "{jsonMessage}");
    static Action<ILogger, string, Exception?> _dubugLoggerMessage = LoggerMessage.Define<string>(LogLevel.Debug, eventId: new EventId(id: 0, name: "Debug"), formatString: "{jsonMessage}");
    static Action<ILogger, string, Exception?> _errorLoggerMessage = LoggerMessage.Define<string>(LogLevel.Error, eventId: new EventId(id: 0, name: "Error"), formatString: "{jsonMessage}");
    static Action<ILogger, string, Exception?> _warningLoggerMessage = LoggerMessage.Define<string>(LogLevel.Warning, eventId: new EventId(id: 0, name: "Warning"), formatString: "{jsonMessage}");
    static Action<ILogger, string, Exception?> _criticalLoggerMessage = LoggerMessage.Define<string>(LogLevel.Critical, eventId: new EventId(id: 0, name: "Critical"), formatString: "{jsonMessage}");


    public static void LogTrace(this ILogger logging, LogStruct logStruct, bool showFullResult = true)
    {
        _traceLoggerMessage(logging, logStruct.ToJson(showFullResult), null);
    }

    public static void LogInformation(this ILogger logging, LogStruct logStruct, bool showFullResult = true)
    {
        _infoLoggerMessage(logging, logStruct.ToJson(showFullResult), null);
    }

    public static void LogDebug(this ILogger logging, LogStruct logStruct, bool showFullResult = true)
    {
        _dubugLoggerMessage(logging, logStruct.ToJson(showFullResult), null);
    }

    public static void LogError(this ILogger logging, LogStruct logStruct, bool showFullResult = true)
    {
        _errorLoggerMessage(logging, logStruct.ToJson(showFullResult), null);
    }

    public static void LogCritical(this ILogger logging, LogStruct logStruct, bool showFullResult = true)
    {
        _criticalLoggerMessage(logging, logStruct.ToJson(showFullResult), null);
    }

    public static void LogWarning(this ILogger logging, LogStruct logStruct, bool showFullResult = true)
    {
        _warningLoggerMessage(logging, logStruct.ToJson(showFullResult), null);
    }
}