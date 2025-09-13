using FluentValidation;
using Microsoft.AspNetCore.Http;
using Shared.Exception.Abstraction;
using System.Net;

namespace Shared.FluentValidation
{
    public class FluentExceptionHandler : ICustomExceptionHandler
    {
        public Dictionary<string, string> Handle(HttpContext httpContext, System.Exception exception)
        {
            Dictionary<string, string> errors = new Dictionary<string, string>();
            if (httpContext != null && exception is ValidationException validationException)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var validationExceptionErrors = validationException.Errors.ToList();
                foreach (var error in validationExceptionErrors)
                {
                    if (errors.ContainsKey(error.PropertyName))
                    {
                        errors[error.PropertyName] = $"{errors[error.PropertyName]} | {error.ErrorMessage}";
                    }
                    else
                    {
                        errors.Add(error.PropertyName, error.ErrorMessage);
                    }
                }
            }

            return errors;
        }
    }
}
