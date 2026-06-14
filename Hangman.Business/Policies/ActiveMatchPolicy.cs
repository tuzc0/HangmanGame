using Hangman.Business.Constants;
using Hangman.DataAccess.Interfaces;
using Hangman.DataAccess.Transporters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hangman.Business.Policies
{
    internal static class ActiveMatchPolicy
    {
        public static async Task<bool> HasActiveMatchAsync(
            IUnitOfWork unitOfWork,
            int playerId)
        {
            List<MatchTransporter> matches =
                await unitOfWork.Matches.GetByPlayerIdAsync(playerId);

            return matches.Any(IsActiveMatch);
        }

        public static async Task<MatchTransporter> GetCurrentActiveMatchAsync(
            IUnitOfWork unitOfWork,
            int playerId)
        {
            List<MatchTransporter> matches =
                await unitOfWork.Matches.GetByPlayerIdAsync(playerId);

            return matches
                .Where(IsActiveMatch)
                .OrderByDescending(match => match.CreatedAt)
                .FirstOrDefault();
        }

        public static async Task<MatchTransporter> GetLatestCreatedLobbyAsync(
            IUnitOfWork unitOfWork,
            int hostId)
        {
            List<MatchTransporter> matches =
                await unitOfWork.Matches.GetByPlayerIdAsync(hostId);

            return matches
                .Where(match =>
                    match.HostId == hostId &&
                    match.MatchStatus == MatchStatusConstants.WaitingForGuest)
                .OrderByDescending(match => match.CreatedAt)
                .FirstOrDefault();
        }

        public static bool IsActiveMatch(MatchTransporter match)
        {
            if (match == null)
            {
                return false;
            }

            return match.MatchStatus == MatchStatusConstants.WaitingForGuest ||
                   match.MatchStatus == MatchStatusConstants.VotingCategory ||
                   match.MatchStatus == MatchStatusConstants.WaitingForHostWord ||
                   match.MatchStatus == MatchStatusConstants.InProgress;
        }
    }
}
