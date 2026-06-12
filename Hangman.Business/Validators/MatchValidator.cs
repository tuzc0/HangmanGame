using Hangman.Business.Messages;
using Hangman.Contracts.Match;

namespace Hangman.Business.Validators
{
    public static class MatchValidator
    {
        public static MatchMessageCode? ValidateCreateLobby(CreateLobbyRequest request)
        {
            if (request == null || request.HostAccountId <= 0)
            {
                return MatchMessageCode.InvalidAccountId;
            }

            return ValidateLanguageCode(request.HostLanguageCode);
        }

        public static MatchMessageCode? ValidateGetAvailableLobbies(
            GetAvailableLobbiesRequest request)
        {
            if (request == null || request.AccountId <= 0)
            {
                return MatchMessageCode.InvalidAccountId;
            }

            return null;
        }

        public static MatchMessageCode? ValidateJoinLobby(JoinLobbyRequest request)
        {
            if (request == null || request.MatchId <= 0)
            {
                return MatchMessageCode.InvalidMatchId;
            }

            if (request.GuestAccountId <= 0)
            {
                return MatchMessageCode.InvalidAccountId;
            }

            return ValidateLanguageCode(request.GuestLanguageCode);
        }

        public static MatchMessageCode? ValidateGetCurrentLobby(
            GetCurrentLobbyRequest request)
        {
            if (request == null || request.AccountId <= 0)
            {
                return MatchMessageCode.InvalidAccountId;
            }

            return null;
        }

        public static MatchMessageCode? ValidateLeaveLobby(
            LeaveLobbyRequest request)
        {
            if (request == null || request.MatchId <= 0)
            {
                return MatchMessageCode.InvalidMatchId;
            }

            if (request.AccountId <= 0)
            {
                return MatchMessageCode.InvalidAccountId;
            }

            return null;
        }

        private static MatchMessageCode? ValidateLanguageCode(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode))
            {
                return MatchMessageCode.InvalidLanguageCode;
            }

            if (languageCode.Trim().Length > 5)
            {
                return MatchMessageCode.InvalidLanguageCode;
            }

            return null;
        }
    }
}
