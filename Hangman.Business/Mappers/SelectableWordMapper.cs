using Hangman.Contracts.Match;
using Hangman.DataAccess.Transporters;

namespace Hangman.Business.Mappers
{
    internal static class SelectableWordMapper
    {
        public static SelectableWordDto ToSelectableWordDto(WordTransporter word)
        {
            if (word == null)
            {
                return null;
            }

            return new SelectableWordDto
            {
                WordId = word.WordId,
                CategoryId = word.CategoryId,
                CategoryName = word.CategoryName,
                WordText = word.WordText,
                Description = word.Description,
                LanguageCode = word.LanguageCode
            };
        }
    }
}
