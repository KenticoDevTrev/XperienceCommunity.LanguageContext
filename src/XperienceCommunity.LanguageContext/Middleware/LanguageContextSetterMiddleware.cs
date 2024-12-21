using CMS.Core;
using Microsoft.AspNetCore.Http;
using XperienceCommunity.LanguageContext.Models;
using XperienceCommunity.LanguageContext.Services;

namespace XperienceCommunity.LanguageContext.Middleware
{
    public class LanguageContextSetterMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext httpContext, 
            IEventLogService eventLogService, 
            ILanguageContextSetter languageContextSetter,
            LanguageContextSetterOptions setterOptions)
        {
            try { 
                await languageContextSetter.SetLanguageContext(setCurrentCulture: setterOptions.SetCurrentCulture);
                await languageContextSetter.SetUserLanguageContext();

            } catch(Exception ex) {
                eventLogService.LogError("LanguageContextSetterMiddleware", "ErrorSettingLanguage", ex.Message);
            }

            await _next(httpContext);
        }
    }
}
