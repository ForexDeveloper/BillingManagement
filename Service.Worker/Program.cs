using Application.Service.HealthChecks;
using Infrastructure.Data.Repository.EfCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service.Worker;
using Service.Worker.Workers;
using Shared.Logging.Abstraction.Models;
using Shared.Logging.Serilog;
using Shared.Logging.Serilog.Configurations;
using Shared.Logging.Serilog.Utilities;
using System;

try
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", false)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}.json", true)
        .Build();

    var builder = Host.CreateDefaultBuilder(args);

    builder
        .AddCustomWorkerSerilog(configuration, cfg =>
        {
            cfg.ApplicationType = ApplicationType.Worker;
            cfg.ApplicationName = configuration["Serilog:ApplicationName"];
            cfg.BaseFilePath = configuration["Serilog:BaseFilePath"];
        })
        .ConfigureServices((hostContext, services) =>
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration["ConnectionStrings:ApplicationDbConnection"],
                    x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "Bill"));
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
            });
            services.AddDbContextPool<ReadonlyApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration["ConnectionStrings:ReadonlyDbConnection"],
                    x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "Bill"));
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            services.AddOptions();
            services.RegisterUnitOfWorks();

            services.RegisterEventBus(hostContext.Configuration);
            services.RegisterOutBoxServices(hostContext.Configuration);
            services.RegisteRedisServices(configuration);
            services.RegisterRepositories();
            services.RegisterServices();


            services.AddHostedService<OutboxPublisherServiceWorker>();
            services.AddHostedService<MerchantBillingServiceWorker>();

            //services.AddHostedService<CustomerWalletServiceWorker>();
            services.AddHealthChecks().AddCheck<DatabaseConnectionHealthCheck>("worker_database_health_check");
        })
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.Configure(app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapHealthChecks("/healthz");
                });
            });
        });

    var app = builder.Build();
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
    SerilogHelpers.WriteLog<HostBuilder>(Serilog.Events.LogEventLevel.Fatal, log);
    throw;
}
finally
{
    SerilogHelpers.FlushLog();
}

