using Microsoft.Extensions.DependencyInjection.Extensions;
using XperienceCommunity.LanguageContext.Models;
using XperienceCommunity.LanguageContext.Repositories;
using XperienceCommunity.LanguageContext.Repositories.Implementations;
using XperienceCommunity.LanguageContext.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionLanguageContextExtensions
    {
        /// <summary>
        /// Adds the LanguageContext systems, use this on your main project to configure how you want languages to be prioritized and set.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="languageContextOptions"></param>
        /// <param name="languageContextSetterOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddLanguageContext(this IServiceCollection services, Action<LanguageContextOptions>? languageContextOptions = null, Action<LanguageContextSetterOptions>? languageContextSetterOptions = null) {
            var setterOptions = new LanguageContextSetterOptions(true);
            languageContextSetterOptions?.Invoke(setterOptions);
            services.AddSingleton(setterOptions);
            
            var languageOptions = new LanguageContextOptions();
            languageContextOptions?.Invoke(languageOptions);
            services.AddSingleton(languageOptions);

            services.AddScoped<ILanguageContextRetriever, LanguageContextRetriever>();
            services.AddScoped<ILanguageContextSetter, LanguageContextSetter>();
            return services;
        }

        /// <summary>
        /// Adds the LanguageContext systems for dependent projects, this will ONLY add if they are not already added, and will only register the ILanguageContextRetriever, not the ILanguageContextSetter
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddLanguageContextForDependentLibrary(this IServiceCollection services)
        {
            services.TryAddScoped<ILanguageContextRetriever, LanguageContextRetriever>();
            return services;
        }
    }
}
