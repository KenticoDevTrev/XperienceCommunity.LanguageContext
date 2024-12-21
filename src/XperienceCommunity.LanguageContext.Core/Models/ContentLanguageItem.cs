namespace XperienceCommunity.LanguageContext.Models
{
    /// <summary>
    /// Kentico agnostic DTO for the ContentLanguageInfo object
    /// </summary>
    /// <param name="LanguageName"></param>
    /// <param name="LanguageID"></param>
    /// <param name="LanguageGuid"></param>
    /// <param name="LanguageDisplayName"></param>
    /// <param name="LanguageCultureCode"></param>
    /// <param name="FlagIconName"></param>
    public record ContentLanguageItem(string LanguageName, int LanguageID, Guid LanguageGuid, string LanguageDisplayName, string LanguageCultureCode, string? FlagIconName = null);
}
