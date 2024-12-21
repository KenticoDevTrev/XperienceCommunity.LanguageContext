using XperienceCommunity.LanguageContext.Middleware;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderLanguageContextExtensions
    {
        /// <summary>
        /// Configures the default Middleware that will retrieve the Context (ILanguageContextRetriever) and User Languages and set them in the HttpContext Items for quicker retriever.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseLanguageContext(this IApplicationBuilder app) => app.UseMiddleware<LanguageContextSetterMiddleware>();
    }
}
