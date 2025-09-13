using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Shared.Logging.Abstraction.Models;

public struct LogStruct
{
    public string Message { get; set; }
    public string ServiceName { get; set; }
    public object InputParams { get; set; }
    public object Results { get; set; }
    public Stopwatch ResponseTimeStopWatcher { get; set; }
    public Exception? Exception { get; set; }
    public LogMessageTag Tags { get; set; }
    public string Method { get; set; }

    public string ToJson(bool showFullResult)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new ExceptionSerializerConverter());
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        try
        {
            string finalResult = JsonSerializer.Serialize(Results, options);

            if (!showFullResult && finalResult?.Length > 500)
            {
                finalResult = "more than 500 character";
            }
            return JsonSerializer.Serialize(new LogDetail
            {
                ResponseTime = Math.Round(ResponseTimeStopWatcher?.Elapsed.TotalMilliseconds ?? 0, MidpointRounding.AwayFromZero),
                ServiceName = ServiceName,
                Method = Method,
                InputParams = JsonSerializer.Serialize(InputParams, options),
                Results = string.IsNullOrEmpty(finalResult) ? string.Empty : finalResult,
                Exception = JsonSerializer.Serialize(Exception, options),
                Message = string.IsNullOrEmpty(Message) && Exception != null ? Exception.Message : Message,
                Tags = Tags.ToString()
            }, options);
        }
        catch (Exception exception)
        {
            return JsonSerializer.Serialize(new LogDetail
            {
                ServiceName = nameof(LogStruct),
                Message = $"An exception occurred in LogStruct serializer for {ServiceName}",
                Exception = JsonSerializer.Serialize(exception, options),
            }, options);
        }
    }
}