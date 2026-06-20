namespace Hangman.Business.Messages
{
    public enum ScoreMessageCode
    {
        Success,
        PlayerNotFound,
        DatabaseConnectionError,
        DatabaseUnavailable,
        DatabaseTimeout,
        ConfigurationError,
        RuntimeError,
        UnexpectedError
    }
}
