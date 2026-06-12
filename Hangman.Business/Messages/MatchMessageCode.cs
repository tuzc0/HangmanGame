namespace Hangman.Business.Messages
{
    public enum MatchMessageCode
    {
        LobbyCreated,
        AvailableLobbiesRetrieved,
        LobbyJoined,
        LobbySubscribed,
        LobbyUnsubscribed,
        LobbySubscriptionFailed,
        LobbyUnsubscriptionFailed,

        AvailableLobbySubscribed,
        AvailableLobbyUnsubscribed,
        AvailableLobbySubscriptionFailed,
        AvailableLobbyUnsubscriptionFailed,

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
        CurrentLobbyRetrieved,
        NoActiveLobby,
        LobbyLeft,
        LobbyLeaveFailed,
        LobbyLeaveNotAllowed,

        DatabaseConnectionError,
        DatabaseTimeout,
        DatabaseUnavailable,
        ConfigurationError,
        RuntimeError,
        UnexpectedError
    }
}
