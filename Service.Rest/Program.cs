using Application.Command.Base;
using Application.Service.HealthChecks;
using Infrastructure.Data.Repository.EfCore;
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
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;


try
{
    var builder = WebApplication.CreateBuilder(args);

    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddUserSecrets(Assembly.GetExecutingAssembly()).Build();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlServer(configuration["ConnectionStrings:ApplicationDbConnection"],
            x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "Bill"));
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
    });

    builder.Services.AddDbContextPool<ReadonlyApplicationDbContext>(options =>
    {
        options.UseSqlServer(configuration["ConnectionStrings:ReadonlyDbConnection"],
            x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "Bill"));
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    });
    builder.AddCustomWebApplicationSerilog(configuration, options =>
    {
        options.ApplicationType = ApplicationType.Api;
        options.ApplicationName = configuration["PublicAppConfiguration:ApplicationName"];
        options.BaseFilePath = configuration["Serilog:BaseFilePath"];
    });

    builder.Services.AddCustomSwagger(cfg =>
    {
        cfg.Title = "Financial Core Manegment Api";
        cfg.IdpServer = new Uri($"{configuration["IDP:Server"]}/connect/token");
    });
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddOptions();
    builder.Services.AddControllers(cfg =>
    {
        cfg.Conventions.AddSwaggerResponseModelConvention();
    })
        .AddCustomFluentValidation(new[] { typeof(BaseCommandValidator<>).Assembly });

    builder.Services.RegisterAuthentication(configuration);
    builder.Services.RegisterMediatorService();
    builder.Services.RegisterRepositories();
    builder.Services.RegisterServices();
    builder.Services.RegisterPublicAppConfiguration(configuration);
    builder.Services.RegisterUnitOfWorks();
    builder.Services.RegisteRedisServices(configuration);
    builder.Services.RegisterOutBoxServices(configuration);
    builder.Services.RegisterMinIoServices(configuration);
    builder.Services.RegisterEncryptionServices(configuration);
    builder.Services.UploadFileConfigurationServices(configuration);

    builder.Services.AddCustomApiVersioning();
    builder.Services.AddHealthChecks().AddCheck<DatabaseConnectionHealthCheck>("database_health_check");


    var app = builder.Build();


    app.UseRequestResponseLogger(cfg =>
    {
        cfg.ExcludedLogPaths = configuration.GetSection("Serilog:ExcludedLogPaths").Get<Collection<string>>();
    });
    app.UseCustomExceptionHandler();
    app.UseCustomSwagger();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseEndpoints(cfg =>
    {
        cfg.MapControllers();
        cfg.MapHealthChecks("/healthz");
    });
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

