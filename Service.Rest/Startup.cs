using Application.Command.Base;
using Application.Service.HealthChecks;
using Infrastructure.Data.Repository.EfCore.DatabaseContexts;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.OpenApi.Models;
using Shared.ApiVersioning;
using Shared.FluentValidation;
using Shared.IdentityServerProvider;
using Shared.Logging.Serilog;
using Shared.Logging.Serilog.Configurations;
using Shared.Middlewares;
using Shared.Swagger;
using Shared.Swagger.Extensions;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Service.Rest
{
    public partial class Startup(IConfiguration configuration, IWebHostEnvironment env)
    {      
        public void ConfigureServices(IServiceCollection services)
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


            services.AddCustomSwagger(cfg =>
            {
                cfg.Title = "Billing Management Api";
                cfg.IdpServer = new Uri($"{configuration["IDP:Server"]}/connect/token");
            });
            services.AddHttpContextAccessor();
            services.AddOptions();
            services.AddControllers(cfg =>
            {
                cfg.Conventions.AddSwaggerResponseModelConvention();
            })
                .AddCustomFluentValidation(new[] { typeof(BaseCommandValidator<>).Assembly });

            services.RegisterAuthentication(configuration);
            services.RegisterMediatorService();
            services.RegisterRepositories();
            services.RegisterServices();
            services.RegisterPublicAppConfiguration(configuration);
            services.RegisterUnitOfWorks();
            services.RegisterRedisServices(configuration);
            services.RegisterOutBoxServices(configuration);
            services.RegisterMinIoServices(configuration);
            services.RegisterEncryptionServices(configuration);
            services.UploadFileConfigurationServices(configuration);

            services.AddCustomApiVersioning();
            services.AddHealthChecks().AddCheck<DatabaseConnectionHealthCheck>("database_health_check");



        }

        public void Configure(IApplicationBuilder app, ILogger<Startup> logger)
        {

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(cfg =>
            {
                cfg.MapControllers();
                cfg.MapHealthChecks("/healthz");
            });
        }
    }
}
