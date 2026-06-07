namespace Hangman.Business.Email
{
    public enum EmailErrorCode
    {
        None = 0,
        RecipientInvalid = 1,
        SmtpConfigurationMissing = 2,
        SmtpConfigurationError = 3,
        SmtpAuthenticationFailed = 4,
        SmtpTimeout = 5,
        SmtpUnavailable = 6,
        SmtpUnknown = 7
    }
}
