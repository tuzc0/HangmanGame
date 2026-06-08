using Hangman.Business.Messages;
using Hangman.Contracts.Word;

namespace Hangman.Business.Validators
{
    public static class WordValidator
    {
        public static WordMessageCode? ValidateGetCategoriesByLanguage(
            GetCategoriesByLanguageRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.LanguageCode))
            {
                return WordMessageCode.InvalidLanguageCode;
            }

            string languageCode = request.LanguageCode.Trim();

            if (languageCode.Length > 5)
            {
                return WordMessageCode.InvalidLanguageCode;
            }

            return null;
        }
    }
}
