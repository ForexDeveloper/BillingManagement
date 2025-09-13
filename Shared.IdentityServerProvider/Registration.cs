using Shared.IdentityServerProvider.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.IdentityServerProvider.Contracts;
using Shared.IdentityServerProvider.Services;

namespace Shared.IdentityServerProvider
{
    public static class Registration
    {
        public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, Action<IdentityServerConfiguration> configurator)
        {
            IdentityServerConfiguration config = new();
            configurator?.Invoke(config);
            services.AddScoped<ICurrentUserService, CurrentUserService>();


            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = config.IdpServer.ToString();
                    options.RequireHttpsMetadata = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateIssuerSigningKey = true,
                    };
                });

            return services;
        }

    }
}