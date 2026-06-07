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
