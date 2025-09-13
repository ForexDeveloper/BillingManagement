using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Shared.Swagger.Helper;
using Shared.Versioning.Abstraction;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shared.Swagger;

public class SwaggerOptionsConfiguration : IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly ICustomApiVersionDescriptionProvider _versionDescriptionProvider;
    private readonly Configurations _configuration;

    public SwaggerOptionsConfiguration(IOptions<Configurations> configuration, ICustomApiVersionDescriptionProvider versionDescriptionProvider = default)
    {
        if (configuration is null) throw new ArgumentNullException(nameof(configuration));

        _versionDescriptionProvider = versionDescriptionProvider;
        _configuration = configuration.Value;
    }

    public void Configure(SwaggerGenOptions options)
    {
        var versions = (_versionDescriptionProvider?.GetDescription() ?? DefaultVersion.GetValue()).ToList();

        foreach (var apiVersion in versions)
        {
            options.SwaggerDoc(apiVersion.GroupName, new OpenApiInfo
            {
                Title = _configuration.Title,
                Version = apiVersion.Version,
                Description = apiVersion.Deprecated ? "This API version has been deprecated." : default,
            });
        }
    }

    public void Configure(string name, SwaggerGenOptions options)
    {
        Configure(options);
    }
}