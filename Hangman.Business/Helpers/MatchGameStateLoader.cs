using Hangman.Business.Mappers;
using Hangman.Contracts.Match;
using Hangman.DataAccess.Interfaces;
using Hangman.DataAccess.Transporters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hangman.Business.Helpers
{
    internal static class MatchGameStateLoader
    {
        public static async Task<MatchGameStateDto> BuildAsync(
            IUnitOfWork unitOfWork,
            MatchTransporter match,
            int currentPlayerId)
        {
            if (match == null)
            {
                return null;
            }

            List<MatchGuessTransporter> letterGuesses =
                await unitOfWork.MatchGuesses.GetByMatchIdAsync(match.MatchId);

            List<MatchWordGuessTransporter> wordGuesses =
                await unitOfWork.MatchWordGuesses.GetByMatchIdAsync(match.MatchId);

            return MatchGameStateMapper.ToGameStateDto(
                match,
                letterGuesses,
                wordGuesses,
                currentPlayerId);
        }

        public static async Task<MatchGameStateDto> BuildByMatchIdAsync(
            IUnitOfWork unitOfWork,
            int matchId,
            int currentPlayerId)
        {
            MatchTransporter match =
                await unitOfWork.Matches.GetByIdAsync(matchId);

            return await BuildAsync(
                unitOfWork,
                match,
                currentPlayerId);
        }
    }
}