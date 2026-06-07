namespace Hangman.Business.Messages
{
    public enum AuthMessageCode
    {
        RegistrationDataRequired,
        LoginDataRequired,
        FullNameRequired,
        InvalidDateOfBirth,
        PhoneRequired,
        InvalidPreferredLanguage,
        InvalidEmail,
        PasswordRequired,
        PasswordTooShort,
        EmailAlreadyRegistered,
        AccountRegisteredEmailVerificationRequired,
        AccountRegisteredVerificationEmailNotSent,
        InvalidEmailOrPassword,
        EmailVerificationRequired,
        AccountNotAvailable,
        AccountNotActive,
        PlayerProfileNotAvailable,
        LoginSuccessful,

        VerificationEmailResendProcessed,
        VerificationEmailResent,
        VerificationEmailResendFailed,
        AccountAlreadyVerified,

        DatabaseConnectionError,
        DatabaseTimeout,
        DatabaseDuplicateKey,
        DatabaseConstraintError,
        DatabaseUnavailable,
        ConfigurationError,
        RuntimeError,
        UnexpectedError
    }
}
