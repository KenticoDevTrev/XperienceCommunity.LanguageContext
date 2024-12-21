using CMS.Base.Internal;
using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Routing;
using Kentico.Content.Web.Mvc.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.LanguageContext.Models;

namespace XperienceCommunity.LanguageContext.Repositories.Implementations
{
    public class LanguageContextRetriever : ILanguageContextRetriever
    {
        public LanguageContextRetriever(IServiceProvider services,
            IWebsiteChannelContext websiteChannelContext,
            IProgressiveCache progressiveCache,
            IInfoProvider<SettingsKeyInfo> settingsKeyInfoProvider,
            IInfoProvider<ContentLanguageInfo> contentLanguageInfoProvider,
            IInfoProvider<WebsiteChannelInfo> websiteChannelInfoProvider,
            IHttpContextAccessor httpContextAccessor)
        {
            // Use if it's been initialized by the user, otherwise fall back to the default language options
            try {
                _languageOptions = services.GetService<LanguageContextOptions>() ?? new LanguageContextOptions();
            } catch (Exception) {
                _languageOptions = new LanguageContextOptions();
            }
            _websiteChannelContext = websiteChannelContext;
            _progressiveCache = progressiveCache;
            _settingsKeyInfoProvider = settingsKeyInfoProvider;
            _contentLanguageInfoProvider = contentLanguageInfoProvider;
            _websiteChannelInfoProvider = websiteChannelInfoProvider;
            _httpContextAccessor = httpContextAccessor;
        }
        private readonly LanguageContextOptions _languageOptions;
        private readonly IWebsiteChannelContext _websiteChannelContext;
        private readonly IProgressiveCache _progressiveCache;
        private readonly IInfoProvider<SettingsKeyInfo> _settingsKeyInfoProvider;
        private readonly IInfoProvider<ContentLanguageInfo> _contentLanguageInfoProvider;
        private readonly IInfoProvider<WebsiteChannelInfo> _websiteChannelInfoProvider;
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
                    // This is actually the primary one Kentico will use
                    if (source == LanguageSource.LanguageRouteOrThreadCulture
                        && (context.Request.RouteValues?.TryGetValue(_languageOptions.LanguageNameRouteValuesKey ?? WebPageRoutingOptions.LANGUAGE_ROUTE_VALUE_KEY, out var languageRouteObj) ?? false)
                        && languageRouteObj != null
                        && languageRouteObj is string languageRouteValue
                        ) {
                        var languageSummary = await GetSummaryFromLanguage(languageRouteValue);
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
                    && _websiteChannelContext.WebsiteChannelID > 0
                    ) {
                    return new LanguageContextSummary(await GetWebsiteLanguage(_websiteChannelContext.WebsiteChannelID), source);
                }
                if (source == LanguageSource.CMSDefault
                    ) {
                    return new LanguageContextSummary(await GetCMSDefault(), source);
                }
            }

            // fall back will always be CMS Default
            return new LanguageContextSummary(await GetCMSDefault(), LanguageSource.CMSDefault);
        }

        private async Task<ContentLanguageItem> GetWebsiteLanguage(int websiteChannelID)
        {
            var defaultLanguage = await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency([$"{WebsiteChannelInfo.OBJECT_TYPE}|all", $"{ContentLanguageInfo.OBJECT_TYPE}|all"]);
                }
                var websiteChannels = (await _websiteChannelInfoProvider.Get()
                .Columns(nameof(WebsiteChannelInfo.WebsiteChannelID), nameof(WebsiteChannelInfo.WebsiteChannelPrimaryContentLanguageID))
                .GetEnumerableTypedResultAsync());

                var languagesById = (await _contentLanguageInfoProvider.Get()
                .GetEnumerableTypedResultAsync())
                .ToDictionary(key => key.ContentLanguageID, value => value);

                return websiteChannels
                    .Where(x => languagesById.ContainsKey(x.WebsiteChannelPrimaryContentLanguageID))
                    .ToDictionary(key => key.WebsiteChannelID, value => ToContentLanguageItem(languagesById[value.WebsiteChannelPrimaryContentLanguageID]));

            }, new CacheSettings(1440, "XperienceCommunity_LanguageContext_GetWebsiteLanguage"));

            return defaultLanguage.TryGetValue(websiteChannelID, out var contentLanguageItem) ? contentLanguageItem : await GetDefaultContentLanguageContentSummary();
        }

        private async Task<ContentLanguageItem> GetCMSDefault()
        {
            var defaultLanguage = await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{SettingsKeyInfo.OBJECT_TYPE}|byname|CMSDefaultCultureCode");
                }
                return (await _settingsKeyInfoProvider.Get()
                .WhereEquals(nameof(SettingsKeyInfo.KeyName), "CMSDefaultCultureCode")
                .GetEnumerableTypedResultAsync())
                .FirstOrDefault()?.KeyValue ?? "en-US";
            }, new CacheSettings(1440, "XperienceCommunity_LanguageContext_GetCMSDefaultLanguage"));

            return (await GetSummaryFromLanguage(defaultLanguage)) ?? await GetDefaultContentLanguageContentSummary();
        }

        private async Task<ContentLanguageItem?> GetSummaryFromLanguage(string language)
        {
            // Get a dictionary that has all possible matches
            var contentLanguageItemsByCodes = await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{ContentLanguageInfo.OBJECT_TYPE}|all");
                }
                var languages = await _contentLanguageInfoProvider.Get()
                .GetEnumerableTypedResultAsync();

                var dictionary = new Dictionary<string, ContentLanguageItem>();
                foreach (var language in languages) {
                    var contentLanguageItem = ToContentLanguageItem(language);
                    dictionary.TryAdd(language.ContentLanguageName.ToLowerInvariant(), contentLanguageItem);
                    dictionary.TryAdd(language.ContentLanguageCultureFormat.ToLowerInvariant(), contentLanguageItem);
                    dictionary.TryAdd(language.ContentLanguageCultureFormat.Split('-')[0].ToLowerInvariant(), contentLanguageItem);
                    dictionary.TryAdd(language.ContentLanguageDisplayName.ToLowerInvariant(), contentLanguageItem);
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

        private async Task<ContentLanguageItem> GetDefaultContentLanguageContentSummary()
        {
            return await _progressiveCache.LoadAsync(async cs => {
                if (cs.Cached) {
                    cs.CacheDependency = CacheHelper.GetCacheDependency($"{ContentLanguageInfo.OBJECT_TYPE}|all");
                }
                var defaultLang = (await _contentLanguageInfoProvider.Get()
                .WhereTrue(nameof(ContentLanguageInfo.ContentLanguageIsDefault))
                .GetEnumerableTypedResultAsync()).FirstOrDefault();

                if (defaultLang != null) {
                    return ToContentLanguageItem(defaultLang);
                }
                var first = (await _contentLanguageInfoProvider.Get()
                .OrderBy(nameof(ContentLanguageInfo.ContentLanguageID))
                .GetEnumerableTypedResultAsync()).FirstOrDefault();
                if (first != null) {
                    return ToContentLanguageItem(first);
                }
                // Should NEVER hit this...
                return new ContentLanguageItem("en", 1, Guid.NewGuid(), "English", "en-US", null);
            }, new CacheSettings(1440, "XperienceCommunity_LanguageContext_GetDefaultContentLanguageContentSummary"));
        }

        private static ContentLanguageItem ToContentLanguageItem(ContentLanguageInfo contentLanguageInfo) => new(
            LanguageName: contentLanguageInfo.ContentLanguageName,
            LanguageID: contentLanguageInfo.ContentLanguageID,
            LanguageGuid: contentLanguageInfo.ContentLanguageGUID,
            LanguageDisplayName: contentLanguageInfo.ContentLanguageDisplayName,
            LanguageCultureCode: contentLanguageInfo.ContentLanguageCultureFormat,
            FlagIconName: string.IsNullOrWhiteSpace(contentLanguageInfo.ContentLanguageFlagIconName) ? (string?)null : contentLanguageInfo.ContentLanguageFlagIconName
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
