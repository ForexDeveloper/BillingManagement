using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Shared.Exception.Abstraction;

namespace Shared.ApiVersioning;
class ApiVersioningErrorProvider : IErrorResponseProvider
{
    public IActionResult CreateResponse(ErrorResponseContext context)
    {
        var code = ushort.TryParse(context.ErrorCode, out var errorCode) ? errorCode : default;
        var errorMessage = new ErrorModel(context.GetType().Name)
        {
            Details = new List<ErrorDetail>()
            {
                new ErrorDetail(context.ErrorCode,context.Message)
            }
        };
        return new BadRequestObjectResult(errorMessage);
    }

}