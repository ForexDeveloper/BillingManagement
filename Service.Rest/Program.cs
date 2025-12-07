using Application.Command.Base;
using Application.Service.HealthChecks;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Service.Rest;
using Shared.ApiVersioning;
using Shared.FluentValidation;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Serilog;
using Shared.Logging.Serilog.Configurations;
using Shared.Logging.Serilog.Utilities;
using Shared.Middlewares;
using Shared.Swagger;
using Shared.Swagger.Extensions;
using System.Collections.ObjectModel;
using System.Reflection;


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

