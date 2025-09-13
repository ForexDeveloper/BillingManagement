using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Exception.Abstraction;
using System.Reflection;

namespace Shared.FluentValidation
{
    public static class Registration
    {
        public static IMvcBuilder AddCustomFluentValidation(this IMvcBuilder mvcBuilder, IEnumerable<Assembly> assemblies)
        {
            mvcBuilder.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblies(assemblies));
            mvcBuilder.Services.AddTransient<ICustomExceptionHandler, FluentExceptionHandler>();
            return mvcBuilder;
        }
    }
}
