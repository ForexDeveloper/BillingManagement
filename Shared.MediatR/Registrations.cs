using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shared.MediatR.Behaviors;
using Shared.MediatR.Configurations;

namespace Shared.MediatR
{
    public static class Registrations
    {
        public static void AddMediatorService(this IServiceCollection services, Action<MediatRConfiguration> configurator)
        {
            if (configurator is null)
                throw new ArgumentNullException(nameof(configurator));

            MediatRConfiguration config = new();
            configurator(config);

            services.Configure(configurator);

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(config.GetAssemblies()));

            if (config.EnableAutoLogging)
                services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LogBehavior<,>));

            if (config.EnableAutoValidation)
                services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        }
    }
}
