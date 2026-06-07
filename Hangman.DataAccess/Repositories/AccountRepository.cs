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
    public class AccountRepository : IAccountRepository
    {
        private readonly HangmanDBEntities context;

        private static readonly Expression<Func<ACCOUNT, AccountTransporter>> AccountProjection =
            account => new AccountTransporter
            {
                AccountId = account.account_id,
                PlayerId = account.player_id,
                Email = account.email,
                IsEmailVerified = account.is_email_verified,
                EmailVerifiedAt = account.email_verified_at,
                AccountStatus = account.account_status,
                CreatedAt = account.created_at,
                UpdatedAt = account.updated_at
            };

        private static readonly Expression<Func<ACCOUNT, AccountCredentialsTransporter>> AccountCredentialsProjection =
            account => new AccountCredentialsTransporter
            {
                AccountId = account.account_id,
                PlayerId = account.player_id,
                Email = account.email,
                PasswordHash = account.password_hash,
                IsEmailVerified = account.is_email_verified,
                AccountStatus = account.account_status
            };

        public AccountRepository(HangmanDBEntities context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<AccountTransporter> GetByIdAsync(int accountId)
        {
            return await context.ACCOUNTs
                .AsNoTracking()
                .Where(account => account.account_id == accountId)
                .Select(AccountProjection)
                .FirstOrDefaultAsync();
        }

        public async Task<AccountTransporter> GetByPlayerIdAsync(int playerId)
        {
            return await context.ACCOUNTs
                .AsNoTracking()
                .Where(account => account.player_id == playerId)
                .Select(AccountProjection)
                .FirstOrDefaultAsync();
        }

        public async Task<AccountTransporter> GetByEmailAsync(string email)
        {
            return await context.ACCOUNTs
                .AsNoTracking()
                .Where(account => account.email == email)
                .Select(AccountProjection)
                .FirstOrDefaultAsync();
        }

        public async Task<AccountCredentialsTransporter> GetCredentialsByEmailAsync(string email)
        {
            return await context.ACCOUNTs
                .AsNoTracking()
                .Where(account => account.email == email)
                .Select(AccountCredentialsProjection)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await context.ACCOUNTs
                .AsNoTracking()
                .AnyAsync(account => account.email == email);
        }

        public void Add(CreateAccountTransporter account)
        {
            ACCOUNT entity = new ACCOUNT
            {
                player_id = account.PlayerId,
                email = account.Email,
                password_hash = account.PasswordHash,
                is_email_verified = account.IsEmailVerified,
                email_verified_at = account.EmailVerifiedAt,
                account_status = account.AccountStatus,
                created_at = DateTime.UtcNow,
                updated_at = null
            };

            context.ACCOUNTs.Add(entity);
        }

        public void AddPendingAccount(CreatePendingAccountTransporter registration)
        {
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            PLAYER playerEntity = new PLAYER
            {
                full_name = registration.FullName,
                date_of_birth = registration.DateOfBirth,
                phone = registration.Phone,
                creation_date = DateTime.UtcNow,
                is_active = registration.IsPlayerActive,
                preferred_language_code = registration.PreferredLanguageCode
            };

            ACCOUNT accountEntity = new ACCOUNT
            {
                PLAYER = playerEntity,
                email = registration.Email,
                password_hash = registration.PasswordHash,
                is_email_verified = registration.IsEmailVerified,
                email_verified_at = registration.EmailVerifiedAt,
                account_status = registration.AccountStatus,
                created_at = DateTime.UtcNow,
                updated_at = null
            };

            EMAIL_VERIFICATION emailVerificationEntity = new EMAIL_VERIFICATION
            {
                ACCOUNT = accountEntity,
                verification_code_hash = registration.VerificationCodeHash,
                expires_at = registration.ExpiresAt,
                verified_at = null,
                attempts = Constants.EmailVerificationDefaults.InitialAttempts,
                is_used = Constants.EmailVerificationDefaults.InitialIsUsed,
                created_at = DateTime.Now
            };

            context.ACCOUNTs.Add(accountEntity);
            context.EMAIL_VERIFICATION.Add(emailVerificationEntity);
        }

        public async Task<bool> MarkEmailAsVerifiedAsync(MarkEmailAsVerifiedTransporter verification)
        {
            if (verification == null)
            {
                throw new ArgumentNullException(nameof(verification));
            }

            ACCOUNT account = await context.ACCOUNTs
                .FirstOrDefaultAsync(item => item.account_id == verification.AccountId);

            if (account == null)
            {
                return false;
            }

            DateTime currentDate = DateTime.UtcNow;

            account.is_email_verified = true;
            account.email_verified_at = currentDate;
            account.account_status = verification.AccountStatus;
            account.updated_at = currentDate;

            return true;
        }

        public async Task<bool> UpdatePasswordHashAsync(int accountId, string passwordHash)
        {
            ACCOUNT account = await context.ACCOUNTs
                .FirstOrDefaultAsync(item => item.account_id == accountId);

            if (account == null)
            {
                return false;
            }

            account.password_hash = passwordHash;
            account.updated_at = DateTime.UtcNow;

            return true;
        }

        public async Task<bool> UpdateStatusAsync(int accountId, string newStatus)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
            {
                throw new ArgumentException("Account status is required.", nameof(newStatus));
            }

            ACCOUNT account = await context.ACCOUNTs
                .FirstOrDefaultAsync(item => item.account_id == accountId);

            if (account == null)
            {
                return false;
            }

            account.account_status = newStatus;
            account.updated_at = DateTime.UtcNow;

            return true;
        }
    }
}
