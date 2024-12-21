namespace XperienceCommunity.LanguageContext.Models
{
    public enum LanguageSource
    {
        /// <summary>
        /// Looks to a query string value for what language the user wants.
        /// </summary>
        QueryString,
        /// <summary>
        /// Looks to a cookie value for what language the user wants.
        /// </summary>
        Cookie,
        /// <summary>
        /// Looks to a header value for what language the user wants.
        /// </summary>
        Header,
        /// <summary>
        /// Xperience by Kentico uses LanguageRoute, ThreadCulture is for KX13
        /// </summary>
        LanguageRouteOrThreadCulture,
        /// <summary>
        /// Uses the System.Globalization.CultureInfo.CurrentCulture
        /// </summary>
        GlobalizationCurrentCulture,
        /// <summary>
        /// Uses the Accepted Languages Header for what the user wants
        /// </summary>
        UserLanguages,
        /// <summary>
        /// The current Website's Default language
        /// </summary>
        WebsiteDefault,
        /// <summary>
        /// The CMS Default Language
        /// </summary>
        CMSDefault
    }
    /*
    public enum LanguageSourceHeadless
    {
        QueryString,
        Cookie,
        Header,
        CMSDefault
    }
    */
}
