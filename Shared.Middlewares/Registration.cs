using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Shared.Middlewares
{
    public static class Registration
    {
        public static void UseRequestResponseLogger(this WebApplication webApplication,
            Action<LoggingMiddlewareConfig> configurator = null!)
        {
            LoggingMiddlewareConfig config = new();
            configurator?.Invoke(config);

            webApplication?.Configuration.Bind(nameof(LoggingMiddlewareConfig), config);
            webApplication?.UseMiddleware<LoggingMiddleware>(config);
        }

        public static void UseRequestResponseLogger(this IApplicationBuilder application,
         IConfiguration configuration,
         Action<LoggingMiddlewareConfig> configurator = null!)
        {
            LoggingMiddlewareConfig config = new();
            configurator?.Invoke(config);
            configuration.Bind(nameof(LoggingMiddlewareConfig), config);
            application?.UseMiddleware<LoggingMiddleware>(config);
        }

        public static void UseCustomExceptionHandler(this WebApplication webApplication)
        {
            webApplication.UseMiddleware<ExceptionHandlerMiddleware>();
        }
    }
}

