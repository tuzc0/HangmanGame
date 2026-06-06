using Hangman.DataAccess.Interfaces;
using Hangman.DataAccess.Model;
using Hangman.DataAccess.Transporters;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Hangman.DataAccess.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly HangmanDBEntities context;

        private static readonly Expression<Func<PLAYER, PlayerTransporter>> PlayerProjection =
            player => new PlayerTransporter
            {
                PlayerId = player.player_id,
                FullName = player.full_name,
                DateOfBirth = player.date_of_birth,
                Phone = player.phone,
                CreationDate = player.creation_date,
                IsActive = player.is_active,
                PreferredLanguageCode = player.preferred_language_code
            };

        public PlayerRepository(HangmanDBEntities context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PlayerTransporter> GetByIdAsync(int playerId)
        {
            return await context.PLAYERs
                .AsNoTracking()
                .Where(player => player.player_id == playerId)
                .Select(PlayerProjection)
                .FirstOrDefaultAsync();
        }

        public async Task<PlayerTransporter> GetByAccountIdAsync(int accountId)
        {
            return await context.ACCOUNTs
                .AsNoTracking()
                .Where(account => account.account_id == accountId)
                .Select(account => account.PLAYER)
                .Select(PlayerProjection)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExistsAsync(int playerId)
        {
            return await context.PLAYERs
                .AsNoTracking()
                .AnyAsync(player => player.player_id == playerId);
        }

        public void Add(CreatePlayerTransporter player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            PLAYER entity = new PLAYER
            {
                full_name = player.FullName,
                date_of_birth = player.DateOfBirth,
                phone = player.Phone,
                creation_date = DateTime.UtcNow,
                is_active = player.IsActive,
                preferred_language_code = player.PreferredLanguageCode
            };

            context.PLAYERs.Add(entity);
        }

        public async Task<bool> UpdateProfileAsync(UpdatePlayerProfileTransporter player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            PLAYER entity = await context.PLAYERs
                .FirstOrDefaultAsync(item => item.player_id == player.PlayerId);

            if (entity == null)
            {
                return false;
            }

            entity.full_name = player.FullName;
            entity.date_of_birth = player.DateOfBirth;
            entity.phone = player.Phone;
            entity.preferred_language_code = player.PreferredLanguageCode;

            return true;
        }

        public async Task<bool> UpdateActiveStatusAsync(UpdatePlayerActiveStatusTransporter player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            PLAYER entity = await context.PLAYERs
                .FirstOrDefaultAsync(item => item.player_id == player.PlayerId);

            if (entity == null)
            {
                return false;
            }

            entity.is_active = player.IsActive;

            return true;
        }
    }
}
