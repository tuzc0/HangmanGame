using Hangman.DataAccess.Interfaces;
using Hangman.DataAccess.Model;
using Hangman.DataAccess.Transporters;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Hangman.DataAccess.Repositories
{
    public class ScoreMovementRepository : IScoreMovementRepository
    {
        private readonly HangmanDBEntities context;

        private static readonly Expression<Func<SCORE_MOVEMENT, ScoreMovementTransporter>> ScoreMovementProjection =
            scoreMovement => new ScoreMovementTransporter
            {
                ScoreMovementId = scoreMovement.score_movement_id,
                PlayerId = scoreMovement.player_id,
                MatchId = scoreMovement.match_id,
                Points = scoreMovement.points,
                MovementType = scoreMovement.movement_type,
                CreatedAt = scoreMovement.created_at
            };

        public ScoreMovementRepository(HangmanDBEntities context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ScoreMovementTransporter> GetByIdAsync(int scoreMovementId)
        {
            return await context.SCORE_MOVEMENT
                .AsNoTracking()
                .Where(scoreMovement => scoreMovement.score_movement_id == scoreMovementId)
                .Select(ScoreMovementProjection)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ScoreMovementTransporter>> GetByPlayerIdAsync(int playerId)
        {
            return await context.SCORE_MOVEMENT
                .AsNoTracking()
                .Where(scoreMovement => scoreMovement.player_id == playerId)
                .OrderByDescending(scoreMovement => scoreMovement.created_at)
                .Select(ScoreMovementProjection)
                .ToListAsync();
        }

        public async Task<List<ScoreMovementTransporter>> GetByMatchIdAsync(int matchId)
        {
            return await context.SCORE_MOVEMENT
                .AsNoTracking()
                .Where(scoreMovement => scoreMovement.match_id == matchId)
                .OrderByDescending(scoreMovement => scoreMovement.created_at)
                .Select(ScoreMovementProjection)
                .ToListAsync();
        }

        public async Task<List<ScoreMovementTransporter>> GetByPlayerIdAndMovementTypeAsync(
            int playerId,
            string movementType)
        {
            return await context.SCORE_MOVEMENT
                .AsNoTracking()
                .Where(scoreMovement =>
                    scoreMovement.player_id == playerId &&
                    scoreMovement.movement_type == movementType)
                .OrderByDescending(scoreMovement => scoreMovement.created_at)
                .Select(ScoreMovementProjection)
                .ToListAsync();
        }

        public async Task<int> GetTotalScoreByPlayerIdAsync(int playerId)
        {
            return await context.SCORE_MOVEMENT
                .AsNoTracking()
                .Where(scoreMovement => scoreMovement.player_id == playerId)
                .Select(scoreMovement => (int?)scoreMovement.points)
                .SumAsync() ?? 0;
        }

        public void Add(CreateScoreMovementTransporter scoreMovement)
        {
            if (scoreMovement == null)
            {
                throw new ArgumentNullException(nameof(scoreMovement));
            }

            SCORE_MOVEMENT entity = new SCORE_MOVEMENT
            {
                player_id = scoreMovement.PlayerId,
                match_id = scoreMovement.MatchId,
                points = scoreMovement.Points,
                movement_type = scoreMovement.MovementType,
                created_at = DateTime.UtcNow
            };

            context.SCORE_MOVEMENT.Add(entity);
        }
    }
}
