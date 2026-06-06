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

        public async Task<List<AvailableMatchTransporter>> GetAvailableByLanguageAsync(
            string matchStatus,
            string languageCode)
        {
            return await
                (from match in context.MATCHes.AsNoTracking()
                 join host in context.PLAYERs.AsNoTracking()
                    on match.host_id equals host.player_id
                 join hostAccount in context.ACCOUNTs.AsNoTracking()
                    on host.player_id equals hostAccount.player_id
                 join word in context.WORDs.AsNoTracking()
                    on match.word_id equals word.word_id
                 join category in context.CATEGORies.AsNoTracking()
                    on word.category_id equals category.category_id
                 where match.match_status == matchStatus
                       && match.guest_id == null
                       && word.language_code == languageCode
                       && word.is_active
                       && category.is_active
                 orderby match.created_at descending
                 select new AvailableMatchTransporter
                 {
                     MatchId = match.match_id,
                     HostId = host.player_id,
                     HostFullName = host.full_name,
                     HostEmail = hostAccount.email,
                     CategoryName = category.name,
                     LanguageCode = word.language_code,
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
                word_id = match.WordId,
                created_at = DateTime.UtcNow,
                joined_at = null,
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
            entity.joined_at = currentDate;
            entity.started_at = currentDate;
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
                join word in context.WORDs.AsNoTracking()
                    on match.word_id equals word.word_id
                join category in context.CATEGORies.AsNoTracking()
                    on word.category_id equals category.category_id
                select new MatchTransporter
                {
                    MatchId = match.match_id,
                    HostId = match.host_id,
                    HostFullName = host.full_name,
                    HostEmail = hostAccount.email,
                    GuestId = match.guest_id,
                    GuestFullName = guest.full_name,
                    GuestEmail = guestAccount.email,
                    WordId = match.word_id,
                    WordText = word.word_text,
                    WordDescription = word.description,
                    CategoryId = category.category_id,
                    CategoryName = category.name,
                    LanguageCode = word.language_code,
                    CreatedAt = match.created_at,
                    JoinedAt = match.joined_at,
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
