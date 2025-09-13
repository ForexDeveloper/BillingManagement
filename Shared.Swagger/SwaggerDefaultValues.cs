using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shared.Swagger;

class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;

        if (operation.Parameters == null)
            return;

        List<OpenApiParameter> parameters = operation.Parameters.ToList();
        foreach (var parameter in parameters)
        {
            var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

            if (parameter.Description == null)
                parameter.Description = description.ModelMetadata?.Description;

            if (parameter.Schema.Default == null && description.DefaultValue != null)
                parameter.Schema.Default = new OpenApiString(description.DefaultValue.ToString());

            parameter.Required |= description.IsRequired;
        }
    }
}