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
        LobbyAbandoned,
        LobbyLeaveNotAllowed,

        CategoryVoteRegistered,
        CategoryVoteUpdated,
        CategoryVotingStateRetrieved,
        CategoryVotingNotActive,
        CategoryVotingExpired,
        CategoryVotingResolved,
        CategoryVotingResolveFailed,

        InvalidCategoryId,
        CategoryNotAvailable,
        NoCategoryVotesAvailable,

        WordSelectionStarted,
        WordSelectionStateRetrieved,
        WordSelectionNotActive,
        WordSelectionNotAllowed,
        WordSelectionExpired,

        InvalidWordId,
        WordNotAvailable,
        WordSelected,
        WordSelectionFailed,

        PlayerNotInMatch,
        PlayerNotHost,
        MatchAlreadyResolved,

        DatabaseConnectionError,
        DatabaseTimeout,
        DatabaseUnavailable,
        ConfigurationError,
        RuntimeError,
        UnexpectedError
    }
}
