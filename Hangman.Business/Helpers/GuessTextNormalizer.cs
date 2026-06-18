using System.Globalization;
using System.Text;

namespace Hangman.Business.Helpers
{
    internal static class GuessTextNormalizer
    {
        private const char UpperEnyePlaceholder = '\uE000';
        private const char LowerEnyePlaceholder = '\uE001';

        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string temporaryValue = value
                .Trim()
                .Replace('Ñ', UpperEnyePlaceholder)
                .Replace('ñ', LowerEnyePlaceholder);

            string decomposedValue =
                temporaryValue.Normalize(NormalizationForm.FormD);

            StringBuilder builder = new StringBuilder();

            foreach (char character in decomposedValue)
            {
                UnicodeCategory category =
                    CharUnicodeInfo.GetUnicodeCategory(character);

                if (category != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(character);
                }
            }

            return builder
                .ToString()
                .Replace(UpperEnyePlaceholder, 'Ñ')
                .Replace(LowerEnyePlaceholder, 'Ñ')
                .ToUpperInvariant();
        }
    }
}
