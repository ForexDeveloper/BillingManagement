using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
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

    public static void SetProperty<TEntity, TProperty>(this TEntity entity, Expression<Func<TEntity, TProperty>> propertyExpression, object? value) where TEntity : class
    {
        var entityType = entity.GetType();

        var expression = (MemberExpression)propertyExpression.Body;

        var property = expression.Member.Name;

        var propertyType = entityType.GetProperty(property)!.PropertyType;

        if (!propertyType.IsClass)
        {
            value = TypeDescriptor.GetConverter(propertyType).ConvertFromInvariantString(value.ToString());
        }

        entityType.GetProperty(property)?.SetValue(entity, value);
    }
}