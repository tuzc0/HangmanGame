using Hangman.Contracts.Match;
using Hangman.DataAccess.Transporters;

namespace Hangman.Business.Mappers
{
    internal static class ScoreMapper
    {
        public static ScoreMovementDto ToScoreMovementDto(ScoreMovementTransporter transporter)
        {
            return new ScoreMovementDto
            {
                ScoreMovementId = transporter.ScoreMovementId,
                Points = transporter.Points,
                MovementType = transporter.MovementType,
                CreatedAt = transporter.CreatedAt
            };
        }
    }
}
