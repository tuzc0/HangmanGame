using Hangman.Contracts.Word;
using System.Threading.Tasks;

namespace Hangman.Business.Interfaces
{
    public interface IWordBusiness
    {
        Task<GetCategoriesByLanguageResponse> GetCategoriesByLanguageAsync(
            GetCategoriesByLanguageRequest request);
    }
}
