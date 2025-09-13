using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Shared.Versioning.Abstraction;

namespace Shared.ApiVersioning
{
    public class ApiVersionDescriptionProvider : ICustomApiVersionDescriptionProvider
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ApiVersionDescriptionProvider(IApiVersionDescriptionProvider versionDescriptionProvider)
        {
            _provider = versionDescriptionProvider;
        }

        public IEnumerable<CustomApiVersionDescriptions> GetDescription()
        {
            var apApiVersionDescriptionsList = _provider.ApiVersionDescriptions.Select(s => new CustomApiVersionDescriptions()
            {
                Version = s.ApiVersion.ToString(),
                Deprecated = s.IsDeprecated,
                GroupName = s.GroupName

            }).ToList();

            return apApiVersionDescriptionsList;
        }
    }
}

