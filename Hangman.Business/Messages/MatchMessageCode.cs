namespace Hangman.Business.Messages
{
    public enum MatchMessageCode
    {
        LobbyCreated,
        AvailableLobbiesRetrieved,
        LobbyJoined,

        InvalidAccountId,
        InvalidMatchId,
        InvalidLanguageCode,

        AccountNotFound,
        AccountNotAvailable,
        EmailVerificationRequired,
        PlayerProfileNotAvailable,

        PlayerAlreadyInActiveMatch,
        MatchNotFound,
        MatchNotAvailable,
        CannotJoinOwnMatch,

        LobbyCreationFailed,
        LobbyJoinFailed,

        DatabaseConnectionError,
        DatabaseTimeout,
        DatabaseUnavailable,
        ConfigurationError,
        RuntimeError,
        UnexpectedError
    }
}
