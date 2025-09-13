using Serilog.Events;
using System.Text.Json;

namespace Shared.Logging.Serilog.Utilities;

static class SerilogEventExtensions
{
    internal static object ExtractProperty(this LogEvent logEvent, string propertyName)
    {
        object propertyValue = string.Empty;
        if (logEvent.Properties.ContainsKey(propertyName))
            propertyValue = ((ScalarValue)logEvent.Properties[propertyName]).Value;

        return propertyValue;
    }

    internal static object ExtractMessage(this LogEvent logEvent)
    {
        try
        {
            if (!logEvent.Properties.ContainsKey("jsonMessage"))
                return string.Empty;

            string jsonStr = ((ScalarValue)logEvent.Properties["jsonMessage"]).Value?.ToString();
            return JsonSerializer.Deserialize<string>(jsonStr ?? string.Empty);
        }
        catch (System.Exception e)
        {
            return new
            {
                exception = JsonSerializer.Serialize(e),
                message = "LogMessageStruct couldn't be extracted."
            };
        }
    }
}
