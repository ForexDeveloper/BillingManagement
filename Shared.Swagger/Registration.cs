using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Shared.Swagger.Helper;
using Shared.Versioning.Abstraction;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Shared.Swagger;

public static class Registration
{
    public static void AddCustomSwagger(this IServiceCollection services, Action<Configurations> configurator = null)
    {
        Configurations config = new();
        configurator?.Invoke(config);

        if (configurator != null)
            services.Configure(configurator);

        services.AddSwaggerGen(options =>
        {
            options.EnableAnnotations();
            options.CustomOperationIds(apiDesc => apiDesc.TryGetMethodInfo(out var methodInfo) ? methodInfo.Name : null);
            options.OperationFilter<SwaggerDefaultValues>();
            options.OperationFilter<AddResponseHeadersFilter>();
            options.OperationFilter<SecurityRequirementsOperationFilter>();

            if (config.IdpServer != default)
            {
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        ClientCredentials = new OpenApiOAuthFlow { TokenUrl = config.IdpServer }
                    }
                });
            }

            options.MapType<DateOnly>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "date"
            });

        });

        services.ConfigureOptions<SwaggerOptionsConfiguration>();
        services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());
        services.AddSwaggerGenNewtonsoftSupport();
    }

    public static void UseCustomSwagger(this WebApplication app)
    {
        if (app is null)
            throw new ArgumentNullException(nameof(app));

        var apiVersionDescriptionProvider = app.Services.GetService<ICustomApiVersionDescriptionProvider>();
        var versions = apiVersionDescriptionProvider?.GetDescription() ?? DefaultVersion.GetValue().ToList();

        _ = app.UseSwagger()
           .UseSwaggerUI(options =>
           {
               options.DisplayOperationId();

               List<CustomApiVersionDescriptions> list = versions.ToList();
               for (int i = 0; i < list.Count; i++)
               {
                   CustomApiVersionDescriptions version = list[i];
                   options.SwaggerEndpoint($"/swagger/{version.GroupName}/swagger.json", version.GroupName.ToUpperInvariant());
               }
           });
    }
}
