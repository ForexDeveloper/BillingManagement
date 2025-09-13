using Serilog.Events;
using Serilog.Formatting;
using Shared.Logging.Serilog.Configurations;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Shared.Logging.Serilog.Utilities;

public class TextFormatter : ITextFormatter
{
    private readonly ApplicationType _applicationType;
    private readonly string _applicationName;

    public TextFormatter(ApplicationType applicationType, string applicationName)
    {
        _applicationType = applicationType;
        _applicationName = applicationName;
    }

    public void Format(LogEvent logEvent, TextWriter output)
    {
        if (logEvent != null)
        {
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string log = JsonSerializer.Serialize(new
            {
                logger = logEvent.ExtractProperty("SourceContext"),
                machineName = logEvent.ExtractProperty("MachineName"),
                threadId = logEvent.ExtractProperty("ThreadId"),
                traceId = logEvent.ExtractProperty("RequestId"),
                clientId = logEvent.ExtractProperty("ClientId"),
                clientIp = logEvent.ExtractProperty("ClientIp"),
                userName = logEvent.ExtractProperty("UserName"),
                xForwardedFor = logEvent.ExtractProperty("x-forwarded-for"),
                message = logEvent.ExtractMessage(),
                time = logEvent.Timestamp.DateTime.ToString("yyyy-MM-dd HH:mm:ss.ffff zzzz", CultureInfo.InvariantCulture),
                level = logEvent.Level == LogEventLevel.Verbose ? "Trace" : logEvent.Level.ToString(),
                applicationName = _applicationName,
                applicationType = _applicationType.ToString(),
                correlationId = logEvent.ExtractProperty("CorrelationId")
            }, options);

            output?.Write(log + output.NewLine);
        }
    }
}