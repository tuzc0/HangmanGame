namespace Hangman.Infrastructure.Email.Builders
{
    public class PasswordResetEmailContext
    {
        public string RecipientEmail { get; set; }

        public string RecipientName { get; set; }

        public string ResetCode { get; set; }

        public int ExpirationMinutes { get; set; }

        public string LanguageCode { get; set; }
    }
}
