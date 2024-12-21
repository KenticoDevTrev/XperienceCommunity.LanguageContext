namespace XperienceCommunity.LanguageContext.Services
{
    public interface ILanguageContextSetter
    {

        /// <summary>
        /// Will preserve the current ILanguageContext.GetLanguageContextSummary in the current HttpRequest.Items for faster lookup.  Call this from Middleware.
        /// </summary>
        /// <param name="setCurrentCulture">If true, will also ensure or alter the current System.Globalization.CultureInfo.CurrentCulture and Thread.CurrentThread.CurrentCulture based on the ContentLanguageCultureFormat</param>
        Task SetLanguageContext(bool setCurrentCulture = true);

        /// <summary>
        /// Will preserve the current ILanguageContext.GetUserLanguages in the current HttpRequest.Items for faster lookup.  Call this from Middleware.
        /// </summary>
        Task SetUserLanguageContext();
    }
}
