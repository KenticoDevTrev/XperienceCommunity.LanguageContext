namespace XperienceCommunity.LanguageContext.Models
{
    /// <summary>
    /// Language Context Setting Options, controls the default Middleware of the Language Context Set
    /// </summary>
    /// <param name="setCurrentCulture"></param>
    public class LanguageContextSetterOptions(bool setCurrentCulture)
    {
        public bool SetCurrentCulture { get; set; } = setCurrentCulture;
    }
}
