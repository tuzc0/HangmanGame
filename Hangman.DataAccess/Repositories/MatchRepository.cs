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
    public class MatchRepository : IMatchRepository
    {
        private readonly HangmanDBEntities context;

        public MatchRepository(HangmanDBEntities context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<MatchTransporter> GetByIdAsync(int matchId)
        {
            return await BuildMatchQuery()
                .Where(match => match.MatchId == matchId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<MatchTransporter>> GetByPlayerIdAsync(int playerId)
        {
            return await BuildMatchQuery()
                .Where(match =>
                    match.HostId == playerId ||
                    match.GuestId == playerId)
                .OrderByDescending(match => match.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<MatchTransporter>> GetByStatusAsync(string matchStatus)
        {
            return await BuildMatchQuery()
                .Where(match => match.MatchStatus == matchStatus)
                .OrderByDescending(match => match.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<AvailableMatchTransporter>> GetAvailableByStatusAsync(string matchStatus)
        {
            return await
                (from match in context.MATCHes.AsNoTracking()
                 join host in context.PLAYERs.AsNoTracking()
                    on match.host_id equals host.player_id
                 join hostAccount in context.ACCOUNTs.AsNoTracking()
                    on host.player_id equals hostAccount.player_id
                 where match.match_status == matchStatus &&
                       match.guest_id == null
                 orderby match.created_at descending
                 select new AvailableMatchTransporter
                 {
                     MatchId = match.match_id,
                     HostId = host.player_id,
                     HostFullName = host.full_name,
                     HostEmail = hostAccount.email,
                     HostLanguageCode = match.host_language_code,
                     MatchStatus = match.match_status,
                     CreatedAt = match.created_at
                 }).ToListAsync();
        }

        public async Task<bool> ExistsAsync(int matchId)
        {
            return await context.MATCHes
                .AsNoTracking()
                .AnyAsync(match => match.match_id == matchId);
        }

        public async Task<bool> IsPlayerInMatchAsync(int matchId, int playerId)
        {
            return await context.MATCHes
                .AsNoTracking()
                .AnyAsync(match =>
                    match.match_id == matchId &&
                    (match.host_id == playerId || match.guest_id == playerId));
        }

        public void Add(CreateMatchTransporter match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            MATCH entity = new MATCH
            {
                host_id = match.HostId,
                guest_id = null,
                selected_category_id = null,
                selected_word_id = null,
                host_language_code = match.HostLanguageCode,
                guest_language_code = null,
                created_at = DateTime.UtcNow,
                joined_at = null,
                category_voting_started_at = null,
                category_voting_ends_at = null,
                word_selection_started_at = null,
                word_selection_ends_at = null,
                started_at = null,
                finished_at = null,
                match_status = match.MatchStatus,
                winner_id = null,
                penalized_user_id = null,
                failed_attempts = match.FailedAttempts,
                max_attempts = match.MaxAttempts
            };

            context.MATCHes.Add(entity);
        }

        public async Task<bool> JoinAsync(JoinMatchTransporter match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            MATCH entity = await context.MATCHes
                .FirstOrDefaultAsync(item => item.match_id == match.MatchId);

            if (entity == null)
            {
                return false;
            }

            DateTime currentDate = DateTime.UtcNow;

            entity.guest_id = match.GuestId;
            entity.guest_language_code = match.GuestLanguageCode;
            entity.joined_at = currentDate;
            entity.category_voting_started_at = match.CategoryVotingStartedAt;
            entity.category_voting_ends_at = match.CategoryVotingEndsAt;
            entity.match_status = match.MatchStatus;

            return true;
        }

        public async Task<bool> UpdateSelectedCategoryAsync(SelectMatchCategoryTransporter match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            MATCH entity = await context.MATCHes
                .FirstOrDefaultAsync(item => item.match_id == match.MatchId);

            if (entity == null)
            {
                return false;
            }

            entity.selected_category_id = match.SelectedCategoryId;
            entity.word_selection_started_at = match.WordSelectionStartedAt;
            entity.word_selection_ends_at = match.WordSelectionEndsAt;
            entity.match_status = match.MatchStatus;

            return true;
        }

        public async Task<bool> UpdateSelectedWordAsync(SelectMatchWordTransporter match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            MATCH entity = await context.MATCHes
                .FirstOrDefaultAsync(item => item.match_id == match.MatchId);

            if (entity == null)
            {
                return false;
            }

            entity.selected_word_id = match.SelectedWordId;
            entity.started_at = DateTime.UtcNow;
            entity.match_status = match.MatchStatus;

            return true;
        }

        public async Task<bool> IncrementFailedAttemptsAsync(int matchId)
        {
            MATCH entity = await context.MATCHes
                .FirstOrDefaultAsync(match => match.match_id == matchId);

            if (entity == null)
            {
                return false;
            }

            entity.failed_attempts++;

            return true;
        }

        public async Task<bool> UpdateStatusAsync(UpdateMatchStatusTransporter match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            MATCH entity = await context.MATCHes
                .FirstOrDefaultAsync(item => item.match_id == match.MatchId);

            if (entity == null)
            {
                return false;
            }

            entity.match_status = match.MatchStatus;

            return true;
        }

        public async Task<bool> FinishAsync(FinishMatchTransporter match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            MATCH entity = await context.MATCHes
                .FirstOrDefaultAsync(item => item.match_id == match.MatchId);

            if (entity == null)
            {
                return false;
            }

            DateTime currentDate = DateTime.UtcNow;

            entity.winner_id = match.WinnerId;
            entity.match_status = match.MatchStatus;
            entity.finished_at = currentDate;

            return true;
        }

        public async Task<bool> RegisterAbandonAsync(AbandonMatchTransporter match)
        {
            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            MATCH entity = await context.MATCHes
                .FirstOrDefaultAsync(item => item.match_id == match.MatchId);

            if (entity == null)
            {
                return false;
            }

            DateTime currentDate = DateTime.UtcNow;

            entity.winner_id = match.WinnerId;
            entity.penalized_user_id = match.PenalizedUserId;
            entity.match_status = match.MatchStatus;
            entity.finished_at = currentDate;

            return true;
        }

        private IQueryable<MatchTransporter> BuildMatchQuery()
        {
            return
                from match in context.MATCHes.AsNoTracking()

                join host in context.PLAYERs.AsNoTracking()
                    on match.host_id equals host.player_id

                join hostAccount in context.ACCOUNTs.AsNoTracking()
                    on host.player_id equals hostAccount.player_id

                join guest in context.PLAYERs.AsNoTracking()
                    on match.guest_id equals guest.player_id into guestGroup
                from guest in guestGroup.DefaultIfEmpty()

                join guestAccount in context.ACCOUNTs.AsNoTracking()
                    on guest.player_id equals guestAccount.player_id into guestAccountGroup
                from guestAccount in guestAccountGroup.DefaultIfEmpty()

                join hostCategoryTranslation in context.CATEGORY_TRANSLATION.AsNoTracking()
                    on new
                    {
                        CategoryId = match.selected_category_id,
                        LanguageCode = match.host_language_code
                    }
                    equals new
                    {
                        CategoryId = (int?)hostCategoryTranslation.category_id,
                        LanguageCode = hostCategoryTranslation.language_code
                    }
                    into hostCategoryTranslationGroup
                from hostCategoryTranslation in hostCategoryTranslationGroup.DefaultIfEmpty()

                join guestCategoryTranslation in context.CATEGORY_TRANSLATION.AsNoTracking()
                    on new
                    {
                        CategoryId = match.selected_category_id,
                        LanguageCode = match.guest_language_code
                    }
                    equals new
                    {
                        CategoryId = (int?)guestCategoryTranslation.category_id,
                        LanguageCode = guestCategoryTranslation.language_code
                    }
                    into guestCategoryTranslationGroup
                from guestCategoryTranslation in guestCategoryTranslationGroup.DefaultIfEmpty()

                join hostWordTranslation in context.WORD_TRANSLATION.AsNoTracking()
                    on new
                    {
                        WordId = match.selected_word_id,
                        LanguageCode = match.host_language_code
                    }
                    equals new
                    {
                        WordId = (int?)hostWordTranslation.word_id,
                        LanguageCode = hostWordTranslation.language_code
                    }
                    into hostWordTranslationGroup
                from hostWordTranslation in hostWordTranslationGroup.DefaultIfEmpty()

                join guestWordTranslation in context.WORD_TRANSLATION.AsNoTracking()
                    on new
                    {
                        WordId = match.selected_word_id,
                        LanguageCode = match.guest_language_code
                    }
                    equals new
                    {
                        WordId = (int?)guestWordTranslation.word_id,
                        LanguageCode = guestWordTranslation.language_code
                    }
                    into guestWordTranslationGroup
                from guestWordTranslation in guestWordTranslationGroup.DefaultIfEmpty()

                select new MatchTransporter
                {
                    MatchId = match.match_id,
                    HostId = match.host_id,
                    HostFullName = host.full_name,
                    HostEmail = hostAccount.email,
                    GuestId = match.guest_id,
                    GuestFullName = guest.full_name,
                    GuestEmail = guestAccount.email,

                    HostLanguageCode = match.host_language_code,
                    GuestLanguageCode = match.guest_language_code,

                    SelectedCategoryId = match.selected_category_id,
                    HostCategoryName = hostCategoryTranslation.name,
                    GuestCategoryName = guestCategoryTranslation.name,

                    SelectedWordId = match.selected_word_id,
                    HostWordText = hostWordTranslation.word_text,
                    GuestWordText = guestWordTranslation.word_text,
                    HostWordDescription = hostWordTranslation.description,
                    GuestWordDescription = guestWordTranslation.description,

                    CreatedAt = match.created_at,
                    JoinedAt = match.joined_at,
                    CategoryVotingStartedAt = match.category_voting_started_at,
                    CategoryVotingEndsAt = match.category_voting_ends_at,
                    WordSelectionStartedAt = match.word_selection_started_at,
                    WordSelectionEndsAt = match.word_selection_ends_at,
                    StartedAt = match.started_at,
                    FinishedAt = match.finished_at,

                    MatchStatus = match.match_status,
                    WinnerId = match.winner_id,
                    PenalizedUserId = match.penalized_user_id,
                    FailedAttempts = match.failed_attempts,
                    MaxAttempts = match.max_attempts
                };
        }
    }
}
