namespace XperienceCommunity.LanguageContext.Models
{
    /// <summary>
    /// Represents the Requested User Languages from the HttpRequest
    /// </summary>
    /// <param name="LanguageCulture">The Culture from the Request</param>
    /// <param name="MatchingLanguageItem">Any matching Content Language Item, will be null if the site does not support this language.</param>
    public record UserLanguages(string LanguageCulture, ContentLanguageItem? MatchingLanguageItem);
}
