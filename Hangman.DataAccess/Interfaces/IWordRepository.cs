using Hangman.DataAccess.Transporters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hangman.DataAccess.Interfaces
{
    public interface IWordRepository
    {
        Task<WordTransporter> GetByIdAsync(int wordId);

        Task<WordTransporter> GetActiveByIdAsync(int wordId);

        Task<List<WordTransporter>> GetActiveByLanguageAsync(string languageCode);

        Task<List<WordTransporter>> GetActiveByCategoryIdAsync(int categoryId);

        Task<List<WordTransporter>> GetActiveByCategoryIdAndLanguageAsync(
            int categoryId,
            string languageCode);

        Task<List<CategoryTransporter>> GetActiveCategoriesByLanguageAsync(string languageCode);

        Task<bool> ExistsActiveAsync(int wordId);
    }
}
