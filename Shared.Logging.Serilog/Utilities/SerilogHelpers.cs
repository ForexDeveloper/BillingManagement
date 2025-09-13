using Serilog;
using Serilog.Events;
using Shared.Logging.Abstraction.Models;

namespace Shared.Logging.Serilog.Utilities;

public static class SerilogHelpers
{
    public static void WriteLog<T>(LogEventLevel logLevel, LogStruct log, bool showFullResult = false)
    {
        Log.ForContext<T>().Write(logLevel, "{@jsonMessage}", log.ToJson(showFullResult));
    }

    public static void FlushLog()
    {
        Log.CloseAndFlush();
    }
}