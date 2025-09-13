using Microsoft.Extensions.DependencyInjection;
using Shared.Middlewares.Models;
using System.Collections.ObjectModel;

namespace Shared.Middlewares.Extensions
{
    public static class MvcBuilderExtensions
    {

        public static IMvcBuilder DefaultValidationAttributeToAPExceptionErrorModel(this IMvcBuilder mvcBuilder)
        {

            mvcBuilder.ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var detail = context.ModelState.Keys
                        .SelectMany(key =>
                            context.ModelState[key]?.Errors
                                .Select(x => new KeyValuePair<string, string>(key, x.ErrorMessage)) ??
                            Array.Empty<KeyValuePair<string, string>>())
                        .ToList();

                    throw new ModelStateValidationException(new Collection<KeyValuePair<string, string>>(detail));
                };
            });
            return mvcBuilder;
        }
    }
}
