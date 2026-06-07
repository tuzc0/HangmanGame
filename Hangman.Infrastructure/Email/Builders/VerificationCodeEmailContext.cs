namespace Hangman.Infrastructure.Email.Builders
{
    internal class VerificationCodeEmailContext
    {
        public string RecipientEmail { get; set; }

        public string RecipientName { get; set; }

        public string VerificationCode { get; set; }

        public int ExpirationMinutes { get; set; }

        public string LanguageCode { get; set; }
    }
}
