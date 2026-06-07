namespace Hangman.Business.Email
{
    public class VerificationEmailRequest
    {
        public string RecipientEmail { get; set; }

        public string RecipientName { get; set; }

        public string VerificationCode { get; set; }

        public int ExpirationMinutes { get; set; }

        public string LanguageCode { get; set; }
    }
}
