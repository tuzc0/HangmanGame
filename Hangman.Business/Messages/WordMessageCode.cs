namespace Hangman.Business.Messages
{
    public enum WordMessageCode
    {
        CategoriesRetrieved,
        InvalidLanguageCode,
        NoCategoriesFound,
        DatabaseConnectionError,
        DatabaseTimeout,
        DatabaseUnavailable,
        ConfigurationError,
        RuntimeError,
        UnexpectedError
    }
}
