namespace Shared.MinIO.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class SensitiveAttribute : Attribute
{
}
