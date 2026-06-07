namespace Hangman.Business.Configuration
{
    public class AuthSettings
    {
        public int PasswordSaltSize { get; set; }

        public int PasswordHashSize { get; set; }

        public int PasswordIterations { get; set; }

        public int PasswordResetExpirationMinutes { get; set; }

        public int VerificationCodeLength { get; set; }

        public int VerificationCodeLimit { get; set; }

        public int EmailVerificationExpirationMinutes { get; set; }

        public int MinimumPasswordLength { get; set; }

        public int MaximumVerificationAttempts { get; set; }

        public string DefaultLanguageCode { get; set; }

        public string[] AllowedLanguageCodes { get; set; }
    }
}
