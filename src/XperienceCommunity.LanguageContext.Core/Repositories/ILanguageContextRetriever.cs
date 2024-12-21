using XperienceCommunity.LanguageContext.Models;

namespace XperienceCommunity.LanguageContext.Repositories
{
    public interface ILanguageContextRetriever
    {
        Task<LanguageContextSummary> GetLanguageContextSummary();

        Task<IEnumerable<UserLanguages>> GetUserLanguages();
    }
}
