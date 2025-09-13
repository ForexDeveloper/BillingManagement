using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Shared.Exception.Abstraction;
using Shared.Exception.Abstraction.Domain;
using Shared.Exception.Abstraction.Infrastructure;
using Shared.Middlewares.Extensions;
using Shared.Middlewares.Models;
using System.Diagnostics;
using System.Text.Json;

namespace Shared.Middlewares;

internal class ExceptionHandlerMiddleware
{
    readonly RequestDelegate _next;
    readonly IWebHostEnvironment _environment;
    readonly IEnumerable<ICustomExceptionHandler> _customExceptionHandlers;

    public ExceptionHandlerMiddleware(RequestDelegate next,
                                         IWebHostEnvironment environment,
                                         IEnumerable<ICustomExceptionHandler> customExceptionHandlers = null!)
    {
        _next = next;
        _environment = environment;
        _customExceptionHandlers = customExceptionHandlers;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        var stopWatch = new Stopwatch();

        try
        {
            stopWatch.Start();
            await _next(httpContext);
        }
        catch (System.Exception exception)
        {
            stopWatch.Stop();
            httpContext.Items.Add("exception", exception);

            httpContext.Response.ContentType = "application/json";

            switch (exception)
            {
                case NotFoundException:
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    break;
                case UnauthorizedException:
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    break;

                case ForbiddenException:
                    httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                    break;

                case BadHttpRequestException:
                case DuplicateException:
                case UnprocessableActionException:
                case ValidationException:
                case ArgumentException:
                case ModelStateValidationException:
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    break;

                case InfrastructureDbOperationException:
                case BaseException:
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    break;

                case TaskCanceledException:
                    httpContext.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                    break;
                default:
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    break;
            }

            List<ErrorDetail> errorDetails = new();
            var customExceptionHandlers = _customExceptionHandlers.ToList();
            foreach (var customExceptionHandler in customExceptionHandlers)
            {
                var details = customExceptionHandler.Handle(httpContext, exception);
                foreach (var detail in details)
                    errorDetails.Add(new ErrorDetail(detail.Key, detail.Value));

            }

            Type exceptionType = exception.GetType();
            string errorType = exceptionType.Name.Replace("Exception", "", StringComparison.InvariantCulture);


            if (exception is BaseException)
            {
                //add exception message to details
                var baseException = exception as BaseException;

                if (baseException?.Details?.Any() == true)
                {
                    foreach (var detail in baseException.Details.Where(c => !string.IsNullOrEmpty(c.Value)).ToList())
                    {
                        errorDetails.Add(new ErrorDetail(detail.Key, detail.Value));
                    }
                }
            }
            else
            {
                if (!_environment.IsProduction() && !errorDetails.Any())
                {
                    var details = exception.GetErrorsFromHierarchy(x => x.InnerException!).ToList();
                    foreach (var detail in details)
                        errorDetails.Add(new ErrorDetail(detail.Key, detail.Value));
                }
            }

            var errorModel = new ErrorModel(errorType)
            {
                Details = errorDetails
            };

            var result = JsonSerializer.Serialize(errorModel);
            await httpContext.Response.WriteAsync(result);

        }
    }


}