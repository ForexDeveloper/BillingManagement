using Shared.Logging.Abstraction.Extensions;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Logging.Abstraction;

internal class ExceptionSerializerConverter : JsonConverter<Exception>
{
    public override Exception Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
    {

        var serializableProperties = new List<KeyValuePair<string, object?>> { new("ClassName", value.GetType().Name) };

        var properties = value.GetType()
            .GetProperties()
            .Where(e => e.Name is nameof(Exception.Message) or nameof(Exception.StackTrace) or nameof(Exception.Source))
            .Select(e => new KeyValuePair<string, object?>(e.Name, e.GetValue(value)));

        serializableProperties.AddRange(properties);

        if (value?.InnerException != null)
        {
            var innerException = value.GetType()
                .GetProperties()
                .Where(e => e.Name is nameof(Exception.InnerException))
                .Select(e => new KeyValuePair<string, object?>(e.Name, value.GetErrorsFromException(x => x.InnerException!)));
            serializableProperties.AddRange(innerException);
        }

        if (options?.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull)
        {
            serializableProperties = serializableProperties.Where(e => e.Value != null).ToList();
        }

        if (serializableProperties.Count == 0)
        {
            // Nothing to write
            return;
        }

        writer.WriteStartObject();

        foreach (var prop in serializableProperties)
        {
            writer.WritePropertyName(prop.Key);
            JsonSerializer.Serialize(writer, prop.Value, options);
        }

        writer.WriteEndObject();

    }
}