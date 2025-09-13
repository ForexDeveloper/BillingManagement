using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Swagger.Extensions
{
    public static class ModelConventionExtensions
    {
        public static void AddSwaggerResponseModelConvention(this IList<IApplicationModelConvention> conventions)
        {
            conventions.Add(new SwaggerResponseModelConvention());
        }
    }
}