namespace Hangman.Business.Messages
{
    public enum ProfileMessageCode
    {
        ProfileDataRequired,
        InvalidAccountId,
        FullNameRequired,
        InvalidDateOfBirth,
        PhoneRequired,
        InvalidPreferredLanguage,

        AccountNotFound,
        AccountNotAvailable,
        EmailVerificationRequired,
        PlayerProfileNotAvailable,

        FullNameTooShort,
        FullNameTooLong,
        PhoneTooShort,
        PhoneTooLong,
        InvalidPhone,

        ProfileRetrieved,
        ProfileUpdated,
        ProfileDeleted,
        ProfileUpdateFailed,
        ProfileDeleteFailed,

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
