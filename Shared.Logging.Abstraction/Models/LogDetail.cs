using System.Text.Json.Serialization;

namespace Shared.Logging.Abstraction.Models;

internal struct LogDetail
{
    [JsonPropertyName("responseTime")]
    public double ResponseTime { get; set; }
    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }
    [JsonPropertyName("method")]
    public string Method { get; set; }
    [JsonPropertyName("inputParams")]
    public string InputParams { get; set; }
    [JsonPropertyName("results")]
    public string Results { get; set; }
    [JsonPropertyName("exception")]
    public string Exception { get; set; }
    [JsonPropertyName("message")]
    public string Message { get; set; }
    [JsonPropertyName("tags")]
    public string Tags { get; set; }
}