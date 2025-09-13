using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Swashbuckle.AspNetCore.Annotations;

namespace Shared.Swagger
{
    public class SwaggerResponseModelConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                var actionReturnType = action.ActionMethod.ReturnType;
                var genericType = actionReturnType.GenericTypeArguments.FirstOrDefault();
                var returnType = genericType?.GenericTypeArguments.FirstOrDefault() ?? genericType ?? actionReturnType;

                if (returnType is not null)
                    action.Filters.Add(new SwaggerResponseAttribute(StatusCodes.Status200OK, null, returnType));

                action.Filters.Add(new SwaggerResponseAttribute(StatusCodes.Status500InternalServerError, "UnHandled Error, please call system admin", typeof(ErrorModel)));
                action.Filters.Add(new SwaggerResponseAttribute(StatusCodes.Status400BadRequest, "BadRequest error, you can read details in returned object", typeof(ErrorModel)));
            }
            catch (Exception)
            {
                // Not Implemented
            }
        }
    }
}