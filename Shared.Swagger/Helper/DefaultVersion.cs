using Shared.Versioning.Abstraction;

namespace Shared.Swagger.Helper
{
    public static class DefaultVersion
    {
        private static CustomApiVersionDescriptions[] Value { get; set; } ={
            new()
            {
                GroupName = "v1",
                Version = "1.0",
                Deprecated = false
            }
        };
        public static CustomApiVersionDescriptions[] GetValue()
        {
            return Value;
        }

    }
}
