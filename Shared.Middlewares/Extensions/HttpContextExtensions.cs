using Microsoft.AspNetCore.Http;

namespace Shared.Middlewares.Extensions;

public static class HttpContextExtensions
{
    public static string GetServiceName(this HttpContext httpContext)
    {
        try
        {
            if (httpContext is null)
                throw new ArgumentNullException(nameof(httpContext));

            var serviceName =
                $"{httpContext.Request.RouteValues["controller"]}/{httpContext.Request.RouteValues["action"]}";
            return (serviceName.Length <= 1 ? httpContext.Request?.Path.Value : serviceName)!;
        }
        catch (System.Exception)
        {
            return default!;
        }
    }

    public static string GetControllerName(this HttpContext httpContext)
    {
        try
        {
            if (httpContext is null)
                return default!;

            var controllerName =
                $"{httpContext.Request.RouteValues["controller"]}";
            return (string.IsNullOrEmpty(controllerName) ? httpContext.Request?.Path.Value : controllerName)!;
        }
        catch (System.Exception)
        {
            return default!;
        }
    }

    public static string GetActionName(this HttpContext httpContext)
    {
        try
        {
            if (httpContext is null)
                return default!;

            var actionName =
                $"{httpContext.Request.RouteValues["action"]}";
            return (string.IsNullOrEmpty(actionName) ? httpContext.Request?.Path.Value : actionName)!;
        }
        catch (System.Exception)
        {
            return default!;
        }
    }

    public static bool IsBusinessError(this HttpResponse httpResponse)
    {
        if (httpResponse?.StatusCode is >= 400 and < 500)
            return true;
        return false;
    }
    public static bool IsTechnicalError(this HttpResponse httpResponse)
    {
        if (httpResponse?.StatusCode is >= 500 and < 600)
            return true;
        return false;
    }
}