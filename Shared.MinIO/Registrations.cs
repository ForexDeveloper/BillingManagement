using Microsoft.Extensions.DependencyInjection;
using Minio;
using Shared.MinIO.Configurations;
using Shared.MinIO.Contracts;
using Shared.MinIO.Services;

namespace Shared.MinIO
{
    public static class Registrations
    {
        public static void AddMinIOService(this IServiceCollection services, Action<MinIOConfiguration> configurator)
        {

            if (configurator is null)
                throw new ArgumentNullException(nameof(configurator));

            MinIOConfiguration config = new();
            configurator(config);
            services.Configure(configurator);

            services.AddScoped<IFileManagerService, FileManagerService>();

            services.AddMinio(options =>
            {
                options.WithEndpoint(config.HostURL);
                options.WithCredentials(config.AccessKey, config.SecretKey);
                options.WithSSL(false);
                options.Build();
            });
        }
    }
}
