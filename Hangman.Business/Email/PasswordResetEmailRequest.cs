namespace Hangman.Business.Email
{
    public class PasswordResetEmailRequest
    {
        public string RecipientEmail { get; set; }

        public string RecipientName { get; set; }

        public string ResetCode { get; set; }

        public int ExpirationMinutes { get; set; }

        public string LanguageCode { get; set; }
    }
}
