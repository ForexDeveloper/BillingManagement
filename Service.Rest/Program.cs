using Service.Rest;
using Shared.Swagger;
using System.Reflection;
using Shared.Middlewares;
using Shared.Logging.Serilog;
using System.Collections.ObjectModel;
using Shared.Logging.Serilog.Utilities;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Serilog.Configurations;

try
{
    var builder = WebApplication.CreateBuilder(args);

    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddUserSecrets(Assembly.GetExecutingAssembly()).Build();

    builder.AddCustomWebApplicationSerilog(configuration, options =>
    {
        options.ApplicationType = ApplicationType.Api;
        options.ApplicationName = configuration["PublicAppConfiguration:ApplicationName"];
        options.BaseFilePath = configuration["Serilog:BaseFilePath"];
    });

    var startup = new Startup(builder.Configuration, builder.Environment);
    startup.ConfigureServices(builder.Services);

    var app = builder.Build();

    app.MapPrometheusScrapingEndpoint("/metrics");

    app.UseRequestResponseLogger(cfg =>
    {
        cfg.ExcludedLogPaths = configuration.GetSection("Serilog:ExcludedLogPaths").Get<Collection<string>>();
    });
    app.UseCustomExceptionHandler();
    app.UseCustomSwagger();

    var logger = app.Services.GetRequiredService<ILogger<Startup>>();
    startup.Configure(app, logger);

    app.Run();
}
catch (Exception exception)
{
    var log = new LogStruct
    {
        ServiceName = "Program",
        Message = "Program Running has error",
        Exception = exception,
    };
    SerilogHelpers.WriteLog<WebApplicationBuilder>(Serilog.Events.LogEventLevel.Fatal, log);
    throw;
}
finally
{
    SerilogHelpers.FlushLog();
}