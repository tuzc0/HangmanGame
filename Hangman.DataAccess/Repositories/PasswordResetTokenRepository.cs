using Hangman.DataAccess.Constants;
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
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly HangmanDBEntities context;

        private static readonly Expression<Func<PASSWORD_RESET_TOKEN, PasswordResetTokenTransporter>>
            PasswordResetTokenProjection =
                token => new PasswordResetTokenTransporter
                {
                    PasswordResetTokenId = token.password_reset_token_id,
                    AccountId = token.account_id,
                    ResetCodeHash = token.reset_code_hash,
                    ExpiresAt = token.expires_at,
                    UsedAt = token.used_at,
                    Attempts = token.attempts,
                    IsUsed = token.is_used,
                    CreatedAt = token.created_at
                };

        public PasswordResetTokenRepository(HangmanDBEntities context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PasswordResetTokenTransporter> GetLatestUnusedByAccountIdAsync(int accountId)
        {
            return await context.PASSWORD_RESET_TOKEN
                .AsNoTracking()
                .Where(token =>
                    token.account_id == accountId &&
                    !token.is_used)
                .OrderByDescending(token => token.created_at)
                .Select(PasswordResetTokenProjection)
                .FirstOrDefaultAsync();
        }

        public void Add(CreatePasswordResetTokenTransporter passwordResetToken)
        {
            if (passwordResetToken == null)
            {
                throw new ArgumentNullException(nameof(passwordResetToken));
            }

            PASSWORD_RESET_TOKEN entity = new PASSWORD_RESET_TOKEN
            {
                account_id = passwordResetToken.AccountId,
                reset_code_hash = passwordResetToken.ResetCodeHash,
                expires_at = passwordResetToken.ExpiresAt,
                used_at = null,
                attempts = PasswordResetTokenDefaults.InitialAttempts,
                is_used = PasswordResetTokenDefaults.InitialIsUsed,
                created_at = DateTime.UtcNow
            };

            context.PASSWORD_RESET_TOKEN.Add(entity);
        }

        public async Task<bool> MarkAsUsedAsync(int passwordResetTokenId)
        {
            PASSWORD_RESET_TOKEN token = await context.PASSWORD_RESET_TOKEN
                .FirstOrDefaultAsync(item => item.password_reset_token_id == passwordResetTokenId);

            if (token == null)
            {
                return false;
            }

            token.is_used = true;
            token.used_at = DateTime.UtcNow;

            return true;
        }

        public async Task<bool> IncrementAttemptsAsync(int passwordResetTokenId)
        {
            PASSWORD_RESET_TOKEN token = await context.PASSWORD_RESET_TOKEN
                .FirstOrDefaultAsync(item => item.password_reset_token_id == passwordResetTokenId);

            if (token == null)
            {
                return false;
            }

            token.attempts++;

            return true;
        }

        public async Task<int> InvalidateUnusedByAccountIdAsync(int accountId)
        {
            PASSWORD_RESET_TOKEN[] unusedTokens = await context.PASSWORD_RESET_TOKEN
                .Where(token =>
                    token.account_id == accountId &&
                    !token.is_used)
                .ToArrayAsync();

            foreach (PASSWORD_RESET_TOKEN token in unusedTokens)
            {
                token.is_used = true;
            }

            return unusedTokens.Length;
        }

        public async Task<int> CleanExpiredUnusedAsync(DateTime expirationLimit)
        {
            PASSWORD_RESET_TOKEN[] expiredTokens = await context.PASSWORD_RESET_TOKEN
                .Where(token =>
                    !token.is_used &&
                    token.expires_at <= expirationLimit)
                .ToArrayAsync();

            foreach (PASSWORD_RESET_TOKEN token in expiredTokens)
            {
                token.is_used = true;
            }

            return expiredTokens.Length;
        }
    }
}
