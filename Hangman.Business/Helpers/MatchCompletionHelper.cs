using Hangman.Business.Constants;
using Hangman.DataAccess.Interfaces;
using Hangman.DataAccess.Transporters;
using System.Threading.Tasks;

namespace Hangman.Business.Helpers
{
    internal static class MatchCompletionHelper
    {
        public static Task<bool> FinishMatchWithoutPenaltyAsync(
            IUnitOfWork unitOfWork,
            int matchId)
        {
            return unitOfWork.Matches.FinishAsync(
                new FinishMatchTransporter
                {
                    MatchId = matchId,
                    WinnerId = null,
                    MatchStatus = MatchStatusConstants.Finished
                });
        }

        public static async Task<bool> RegisterPenalizedAbandonAsync(
            IUnitOfWork unitOfWork,
            int matchId,
            int penalizedPlayerId,
            int winnerPlayerId)
        {
            bool abandoned = await unitOfWork.Matches.RegisterAbandonAsync(
                new AbandonMatchTransporter
                {
                    MatchId = matchId,
                    PenalizedUserId = penalizedPlayerId,
                    WinnerId = winnerPlayerId,
                    MatchStatus = MatchStatusConstants.Abandoned
                });

            if (!abandoned)
            {
                return false;
            }

            unitOfWork.ScoreMovements.Add(
                new CreateScoreMovementTransporter
                {
                    PlayerId = penalizedPlayerId,
                    MatchId = matchId,
                    Points = ScorePointsConstants.AbandonPenalty,
                    MovementType = ScoreMovementTypeConstants.AbandonPenalty
                });

            return true;
        }
    }
}
