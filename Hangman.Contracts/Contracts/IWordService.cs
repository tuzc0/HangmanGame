using Hangman.Contracts.Word;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Contracts.Contracts
{
    [ServiceContract]
    public interface IWordService
    {
        [OperationContract]
        Task<GetCategoriesByLanguageResponse> GetCategoriesByLanguageAsync(
            GetCategoriesByLanguageRequest request);
    }
}
