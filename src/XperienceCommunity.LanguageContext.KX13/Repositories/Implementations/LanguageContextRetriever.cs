using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.SiteProvider;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.LanguageContext.Models;

namespace XperienceCommunity.LanguageContext.Repositories.Implementations
{
    public class LanguageContextRetriever : ILanguageContextRetriever
    {
        public LanguageContextRetriever(IServiceProvider services,
            ISiteService siteService,
            IProgressiveCache progressiveCache,
            IInfoProvider<SiteInfo> siteInfoProvider,
            IInfoProvider<CultureInfo> cultureInfoProvider,
            IHttpContextAccessor httpContextAccessor)
        {
            // Use if it's been initialized by the user, otherwise fall back to the default language options
            try {
                _languageOptions = services.GetService<LanguageContextOptions>() ?? new LanguageContextOptions();
            } catch (Exception) {
                _languageOptions = new LanguageContextOptions();
            }
            _siteService = siteService;
            _progressiveCache = progressiveCache;
            _siteInfoProvider = siteInfoProvider;
            _cultureInfoProvider = cultureInfoProvider;
            _httpContextAccessor = httpContextAccessor;
        }
        private readonly LanguageContextOptions _languageOptions;
        private readonly ISiteService _siteService;
        private readonly IProgressiveCache _progressiveCache;
        private readonly IInfoProvider<SiteInfo> _siteInfoProvider;
        private readonly IInfoProvider<CultureInfo> _cultureInfoProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public async Task<LanguageContextSummary> GetLanguageContextSummary()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null) {
                // first check HttpContext Items if summary is already set
                if (context.Items != null && context.Items.ContainsKey(LanguageContextHttpContextItemNames._languageSummaryHttpItemName)
                    && context.Items[LanguageContextHttpContextItemNames._languageSummaryHttpItemName] is LanguageContextSummary summary) {
                    return summary;
                }
            }

            foreach (var source in _languageOptions.SourcePriority) {

                if (context != null && context.Request != null) {
                    if (source == LanguageSource.QueryString
                        && !string.IsNullOrWhiteSpace(_languageOptions.QueryStringName)
                        && (context.Request.Query?.ContainsKey(_languageOptions.QueryStringName) ?? false)) {
                        var languageSummary = await GetSummaryFromLanguage(context.Request.Query[_languageOptions.QueryStringName].First() ?? "");
                        if (languageSummary != null) {
                            return new LanguageContextSummary(languageSummary, source);
                        }
                    }
                    if (source == LanguageSource.Cookie
                        && !string.IsNullOrWhiteSpace(_languageOptions.CookieName)
                        && (context.Request.Cookies?.Keys.Contains(_languageOptions.CookieName) ?? false)) {
                        var languageSummary = await GetSummaryFromLanguage(context.Request.Cookies[_languageOptions.CookieName] ?? "");
                        if (languageSummary != null) {
                            return new LanguageContextSummary(languageSummary, source);
                        }
                    }
                    if (source == LanguageSource.Header
                        && !string.IsNullOrWhiteSpace(_languageOptions.HeaderName)
                        && (context.Request.Headers?.TryGetValue(_languageOptions.HeaderName, out var languageHeaderVal) ?? false)
                        && languageHeaderVal.Count != 0
                        ) {
                        var languageSummary = await GetSummaryFromLanguage(languageHeaderVal.FirstOrDefault() ?? "");
                        if (languageSummary != null) {
                            return new LanguageContextSummary(languageSummary, source);
                        }
                    }
                    /* For KX 13, the Language Route is synonymous with the Current Culture using the Route Constraint
                    /* https://docs.kentico.com/13/multilingual-websites/setting-up-multilingual-websites/setting-up-culture-detection
                    */
                    if (source == LanguageSource.LanguageRouteOrThreadCulture
                        && !string.IsNullOrWhiteSpace(Thread.CurrentThread.CurrentUICulture?.Name)
                        ) {
                        var languageSummary = await GetSummaryFromLanguage(Thread.CurrentThread.CurrentUICulture.Name);
                        if (languageSummary != null) {
                            return new LanguageContextSummary(languageSummary, source);
                        }
                    }
                    if (source == LanguageSource.GlobalizationCurrentCulture
                        && !string.IsNullOrWhiteSpace(System.Globalization.CultureInfo.CurrentCulture?.Name ?? "")
                        ) {
                        var languageSummary = await GetSummaryFromLanguage(System.Globalization.CultureInfo.CurrentCulture?.Name ?? "");
                        if (languageSummary != null) {
                            return new LanguageContextSummary(languageSummary, source);
                        }
                    }
                    if (source == LanguageSource.UserLanguages) {
                        var validUserLanguages = (await GetUserLanguages()).Where(x => x.MatchingLanguageItem != null).FirstOrDefault();
                        if (validUserLanguages != null && validUserLanguages.MatchingLanguageItem != null) {
                            return new LanguageContextSummary(validUserLanguages.MatchingLanguageItem, source);
                        }
                    }
                }
                if (source == LanguageSource.WebsiteDefault
                    && _siteService.CurrentSite.SiteID > 0
                    ) {
                    return new LanguageContextSummary(await GetWebsiteLanguage(_siteService.CurrentSite.SiteID), source);
                }
                if (source == LanguageSource.CMSDefault
                    ) {
                    return new LanguageContextSummary(await GetCMSDefault(), source);
                }
            }

            // fall back will always be CMS Default
            return new LanguageContextSummary(await GetCMSDefault(), LanguageSource.CMSDefault);
        }

        private async Task<ContentLanguageItem> GetWebsiteLanguage(int siteID)
        {
            var defaultCMSLanguage = await GetCMSDefault();
            var defaultLanguage = await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency([$"{SiteInfo.OBJECT_TYPE}|all", $"{CultureInfo.OBJECT_TYPE}|all"]);
                }
                var sites = (await _siteInfoProvider.Get()
                .Columns(nameof(SiteInfo.SiteID), "SiteDefaultVisitorCulture")
                .GetEnumerableTypedResultAsync());
                var dictionary = new Dictionary<int, ContentLanguageItem>();
                foreach (var site in sites) {
                    if (!string.IsNullOrWhiteSpace(site.DefaultVisitorCulture)) {
                        dictionary.Add(site.SiteID, (await GetSummaryFromLanguage(site.DefaultVisitorCulture)) ?? defaultCMSLanguage);
                    } else {
                        dictionary.Add(site.SiteID, defaultCMSLanguage);
                    }
                }
                return dictionary;
            }, new CacheSettings(1440, "XperienceCommunity_LanguageContext_GetWebsiteLanguage", defaultCMSLanguage.LanguageCultureCode));

            return defaultLanguage.TryGetValue(siteID, out var contentLanguageItem) ? contentLanguageItem : await GetCMSDefault();
        }

        private async Task<ContentLanguageItem> GetCMSDefault()
        {
            int siteID = _siteService.CurrentSite.SiteID;
            var defaultLanguage = _progressiveCache.Load(cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{SettingsKeyInfo.OBJECT_TYPE}|byname|CMSDefaultCultureCode");
                }
                return SettingsKeyInfoProvider.GetSettingsKeyInfo("CMSDefaultCultureCode", siteID)?.KeyValue ?? "en-US";
            }, new CacheSettings(1440, "XperienceCommunity_LanguageContext_GetCMSDefaultLanguage", siteID));
            // Should always find a match here, the empty one shouldn't ever hit.
            return (await GetSummaryFromLanguage(defaultLanguage)) ?? new ContentLanguageItem("en-US", 0, Guid.Empty, "English", "en-US", null);
        }

        private async Task<ContentLanguageItem?> GetSummaryFromLanguage(string language)
        {
            // Get a dictionary that has all possible matches
            var contentLanguageItemsByCodes = await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{CultureInfo.OBJECT_TYPE}|all");
                }
                var languages = await _cultureInfoProvider.Get()
                .GetEnumerableTypedResultAsync();

                var dictionary = new Dictionary<string, ContentLanguageItem>();
                foreach (var language in languages) {
                    var contentLanguageItem = ToContentLanguageItem(language);
                    dictionary.TryAdd(language.CultureCode.ToLowerInvariant(), contentLanguageItem);
                    dictionary.TryAdd(language.CultureCode.Split('-')[0].ToLowerInvariant(), contentLanguageItem);
                    dictionary.TryAdd(language.CultureShortName.ToLowerInvariant(), contentLanguageItem);
                    dictionary.TryAdd(language.CultureName.ToLowerInvariant(), contentLanguageItem);
                }

                return dictionary;
            }, new CacheSettings(1440, "XperienceCommunity_LanguageContext_GetSummaryFromLanguage"));

            // Try to find match
            var lookup = language.ToLowerInvariant().Trim();
            var lookupTwo = lookup.Split('-')[0];
            if (contentLanguageItemsByCodes.TryGetValue(lookup, out var contentLanguageByLookup)) {
                return contentLanguageByLookup;
            }
            if (contentLanguageItemsByCodes.TryGetValue(lookupTwo, out var contentLanguageByLookup2)) {
                return contentLanguageByLookup2;
            }
            return null;
        }

        private static ContentLanguageItem ToContentLanguageItem(CultureInfo cultureInfo) => new(
            LanguageName: cultureInfo.CultureCode,
            LanguageID: cultureInfo.CultureID,
            LanguageGuid: cultureInfo.CultureGUID,
            LanguageDisplayName: cultureInfo.CultureShortName,
            LanguageCultureCode: cultureInfo.CultureCode,
            FlagIconName: null
            );

        public async Task<IEnumerable<UserLanguages>> GetUserLanguages()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null && context.Request != null) {

                // first check HttpContext Items if summary is already set
                if (context.Items != null && context.Items.ContainsKey(LanguageContextHttpContextItemNames._UserLanguagesHttpItemName)
                        && context.Items[LanguageContextHttpContextItemNames._UserLanguagesHttpItemName] is List<UserLanguages> userLanguagesFound) {
                    return userLanguagesFound;
                }

                // Look to header to find any matches
                var languages = context.Request.GetTypedHeaders().AcceptLanguage;
                if (languages == null) {
                    return [];
                }
                var languageCodes = languages.OrderByDescending(x => x.Quality ?? 1)
                    .Select(x => x.Value.ToString());

                // Add User Languages and any matching items found
                var userLanguages = new List<UserLanguages>();
                foreach (var language in languageCodes) {
                    var languageFound = await GetSummaryFromLanguage(language);
                    userLanguages.Add(new UserLanguages(language, languageFound));
                }
                return userLanguages;
            }

            return [];
        }
    }
}
