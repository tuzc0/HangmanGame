using Hangman.DataAccess.Transporters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hangman.DataAccess.Interfaces
{
    public interface IWordRepository
    {
        Task<WordTransporter> GetByIdAsync(int wordId, string languageCode);

        Task<WordTransporter> GetActiveByIdAsync(int wordId, string languageCode);

        Task<List<WordTransporter>> GetActiveByLanguageAsync(string languageCode);

        Task<List<WordTransporter>> GetActiveByCategoryIdAndLanguageAsync(
            int categoryId,
            string languageCode);

        Task<List<CategoryTransporter>> GetActiveCategoriesByLanguageAsync(string languageCode);

        Task<bool> ExistsActiveAsync(int wordId);

        Task<bool> ExistsActiveCategoryAsync(int categoryId);

        Task<bool> ExistsActiveTranslationAsync(int wordId, string languageCode);
    }
}
