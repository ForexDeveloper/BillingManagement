using System.Text;
using System.Text.Json;

namespace Rest.Integration.Tests.Base;

public static class UtilityExtension
{
    public static StringContent ToStringContent(this object model)
    {
        var json = JsonSerializer.Serialize(model);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public static string GenerateUniqueInvoiceNumber()
    {
        return $"int-test-{DateTime.UtcNow:HHmmssfff}";
    }

    public static void SetProperty(this object entity, string property, object? value)
    {
        var entityType = entity.GetType();

        entityType.GetProperty(property)?.SetValue(entity, value);
    }
}