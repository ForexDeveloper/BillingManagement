using Microsoft.AspNetCore.Http;

namespace Shared.Exception.Abstraction
{
    public interface ICustomExceptionHandler
    {
        Dictionary<string, string> Handle(HttpContext httpContext, System.Exception exception);
    }
}
