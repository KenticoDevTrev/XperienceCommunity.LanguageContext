using CMS.Base.Internal;
using System.Globalization;
using XperienceCommunity.LanguageContext.Models;
using XperienceCommunity.LanguageContext.Services;

namespace XperienceCommunity.LanguageContext.Repositories.Implementations
{
    public class LanguageContextSetter(ILanguageContextRetriever languageContext,
                    IHttpContextRetriever httpContextRetriever) : ILanguageContextSetter
    {
        private readonly ILanguageContextRetriever _languageContext = languageContext;
        private readonly IHttpContextRetriever _httpContextRetriever = httpContextRetriever;

        public async Task SetLanguageContext(bool setCurrentCulture = true)
        {
            var currentCultureSummary = await _languageContext.GetLanguageContextSummary();
            var context = _httpContextRetriever.GetContext();
            if(context != null) {
                context.Items[LanguageContextHttpContextItemNames._languageSummaryHttpItemName] = currentCultureSummary;
            }
            if (setCurrentCulture) {
                try { 
                    CultureInfo.CurrentCulture = new CultureInfo(currentCultureSummary.RequestLanguage.LanguageCultureCode);
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(currentCultureSummary.RequestLanguage.LanguageCultureCode);
                } catch(Exception) {

                }
            }
        }

        public async Task SetUserLanguageContext()
        {
            var userLanguages = await _languageContext.GetUserLanguages();
            var context = _httpContextRetriever.GetContext();
            if (context != null) {
                context.Items[LanguageContextHttpContextItemNames._UserLanguagesHttpItemName] = userLanguages;
            }
        }
    }
}
