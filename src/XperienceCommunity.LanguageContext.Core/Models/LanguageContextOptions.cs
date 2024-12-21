namespace XperienceCommunity.LanguageContext.Models
{
    public class LanguageContextOptions
    {
        /// <summary>
        /// Set the priorities that you wish for the language to be determined.  Standard Priority is QueryString (if the QueryStringName value is set), Cookie (if the CookieName value is set), Header (if the HeaderName value is set), LanguageRouteOrThreadCulture (XbyK LanguageRoute, KX13 ThreadCulture), CurrentCulture, WebsiteDefault (fallback), CMSDefault (absolute fallback)
        /// </summary>
        public IEnumerable<LanguageSource> SourcePriority { get; set; } = [LanguageSource.QueryString, LanguageSource.Cookie, LanguageSource.Header, LanguageSource.LanguageRouteOrThreadCulture, LanguageSource.GlobalizationCurrentCulture, LanguageSource.WebsiteDefault, LanguageSource.CMSDefault];

        /// <summary>
        /// If set, what query string value it will look for for a language key.  Example ?language=es.  It is your responsibility to provide the mechanism to retrieve this value.
        /// </summary>
        public string? QueryStringName { get; set; }

        /// <summary>
        /// If set, what Cookie Name it will look for the language value.  It is your responsibility to set this cookie.
        /// </summary>
        public string? CookieName { get; set; }

        /// <summary>
        /// If set, what Header Name it will look for the language value.  It is your responsibility to set this header in requests.
        /// </summary>
        public string? HeaderName { get; set; }

        /// <summary>
        /// If you set a different WebPageRoutingOption in your startup, can set it here, will default to the WebPageRoutingOptions.LANGUAGE_ROUTE_VALUE_KEY (kxpLanguage).
        /// </summary>
        public string? LanguageNameRouteValuesKey { get; set; }
    }
}
