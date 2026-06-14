using Hangman.DataAccess.Interfaces;
using Hangman.DataAccess.Model;
using Hangman.DataAccess.Transporters;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Hangman.DataAccess.Repositories
{
    public class MatchCategoryVoteRepository : IMatchCategoryVoteRepository
    {
        private readonly HangmanDBEntities context;

        public MatchCategoryVoteRepository(HangmanDBEntities context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<MatchCategoryVoteTransporter> GetByMatchAndPlayerAsync(
            int matchId,
            int playerId)
        {
            return await context.MATCH_CATEGORY_VOTE
                .AsNoTracking()
                .Where(vote =>
                    vote.match_id == matchId &&
                    vote.player_id == playerId)
                .Select(vote => new MatchCategoryVoteTransporter
                {
                    MatchCategoryVoteId = vote.match_category_vote_id,
                    MatchId = vote.match_id,
                    PlayerId = vote.player_id,
                    CategoryId = vote.category_id,
                    CreatedAt = vote.created_at,
                    UpdatedAt = vote.updated_at
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<MatchCategoryVoteTransporter>> GetByMatchIdAsync(
            int matchId)
        {
            return await context.MATCH_CATEGORY_VOTE
                .AsNoTracking()
                .Where(vote => vote.match_id == matchId)
                .OrderBy(vote => vote.created_at)
                .Select(vote => new MatchCategoryVoteTransporter
                {
                    MatchCategoryVoteId = vote.match_category_vote_id,
                    MatchId = vote.match_id,
                    PlayerId = vote.player_id,
                    CategoryId = vote.category_id,
                    CreatedAt = vote.created_at,
                    UpdatedAt = vote.updated_at
                })
                .ToListAsync();
        }

        public async Task<List<MatchCategoryVoteTransporter>> GetByMatchIdAsync(
            int matchId,
            string languageCode)
        {
            return await
                (from vote in context.MATCH_CATEGORY_VOTE.AsNoTracking()
                 join translation in context.CATEGORY_TRANSLATION.AsNoTracking()
                    on vote.category_id equals translation.category_id
                 where vote.match_id == matchId &&
                       translation.language_code == languageCode
                 orderby vote.created_at
                 select new MatchCategoryVoteTransporter
                 {
                     MatchCategoryVoteId = vote.match_category_vote_id,
                     MatchId = vote.match_id,
                     PlayerId = vote.player_id,
                     CategoryId = vote.category_id,
                     CategoryName = translation.name,
                     LanguageCode = translation.language_code,
                     CreatedAt = vote.created_at,
                     UpdatedAt = vote.updated_at
                 }).ToListAsync();
        }

        public async Task<bool> UpsertAsync(SaveMatchCategoryVoteTransporter vote)
        {
            if (vote == null)
            {
                throw new ArgumentNullException(nameof(vote));
            }

            MATCH_CATEGORY_VOTE entity = await context.MATCH_CATEGORY_VOTE
                .FirstOrDefaultAsync(item =>
                    item.match_id == vote.MatchId &&
                    item.player_id == vote.PlayerId);

            DateTime currentDate = DateTime.UtcNow;

            if (entity == null)
            {
                entity = new MATCH_CATEGORY_VOTE
                {
                    match_id = vote.MatchId,
                    player_id = vote.PlayerId,
                    category_id = vote.CategoryId,
                    created_at = currentDate,
                    updated_at = null
                };

                context.MATCH_CATEGORY_VOTE.Add(entity);
                return true;
            }

            entity.category_id = vote.CategoryId;
            entity.updated_at = currentDate;

            return true;
        }
    }
}
