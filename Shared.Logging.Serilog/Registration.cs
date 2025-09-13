using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Enrichers.Sensitive;
using Shared.Logging.Serilog.Configurations;
using Shared.Logging.Serilog.Enrichers;
using Shared.Logging.Serilog.MaskOperations;

namespace Shared.Logging.Serilog;

public static class Registration
{
    public static WebApplicationBuilder AddCustomWebApplicationSerilog(this WebApplicationBuilder webAppBuilder, IConfiguration configuration, Action<SerilogConfiguration> configurator)
    {
        SerilogConfiguration config = new();

        if (configurator != null)
        {
            configurator(config);
        }


        var logger = new LoggerConfiguration().ReadFrom
            .Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithMachineName()
            .Enrich.WithClientIp()
            .Enrich.WithCorrelationId()
            .Enrich.WithCorrelationIdHeader()
            .Enrich.With<IdentityUserEnricher>()
            .Enrich.With<XForwardedForEnricher>()
            .WriteTo.Async(wt => wt.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));


        if (config.MaskConfig.Enabled)
        {
            List<IMaskingOperator> maskingOperators = new List<IMaskingOperator>();
            if (config.MaskConfig.MaskIban)
                maskingOperators.Add(new CustomIbanMaskingOperator());

            if (config.MaskConfig.MaskPan)
                maskingOperators.Add(new PanMaskingOperator());

            if (!string.IsNullOrEmpty(config.MaskConfig.CustomRegex))
                maskingOperators.Add(new GeneralRegexMaskingOperator(config.MaskConfig.CustomRegex));

            logger.Enrich.WithSensitiveDataMasking(
            options =>
            {
                options.MaskingOperators = maskingOperators;
            });
        }

        Log.Logger = logger.CreateLogger();

        if (webAppBuilder != null)
        {
            webAppBuilder.Services.AddScoped<XForwardedForEnricher>();
            webAppBuilder.Host.UseSerilog();
        }
        return webAppBuilder;
    }

    public static IHostBuilder AddCustomWorkerSerilog(this IHostBuilder webAppBuilder, IConfiguration configuration, Action<SerilogConfiguration> configurator)
    {
        SerilogConfiguration config = new();
        if (configurator != null)
        {
            configurator(config);
        }

        var logger = new LoggerConfiguration().ReadFrom
                                              .Configuration(configuration)
                                              .Enrich.FromLogContext()
                                              .Enrich.WithThreadId()
                                              .Enrich.WithMachineName()
                                              .Enrich.WithClientIp()
            .WriteTo.Async(wt => wt.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));
        // .WriteTo.Console(new RenderedCompactJsonFormatter());



        if (config.MaskConfig.Enabled)
        {
            List<IMaskingOperator> maskingOperators = new List<IMaskingOperator>();
            if (config.MaskConfig.MaskIban)
                maskingOperators.Add(new CustomIbanMaskingOperator());

            if (config.MaskConfig.MaskPan)
                maskingOperators.Add(new PanMaskingOperator());

            if (!string.IsNullOrEmpty(config.MaskConfig.CustomRegex))
                maskingOperators.Add(new GeneralRegexMaskingOperator(config.MaskConfig.CustomRegex));

            logger.Enrich.WithSensitiveDataMasking(
            options =>
            {
                options.MaskingOperators = maskingOperators;
            });
        }


        Log.Logger = logger.CreateLogger();

        webAppBuilder.UseSerilog();

        return webAppBuilder;
    }
}
