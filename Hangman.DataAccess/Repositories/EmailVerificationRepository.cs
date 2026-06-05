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
    public class EmailVerificationRepository : IEmailVerificationRepository
    {
        private readonly HangmanDBEntities context;

        private static readonly Expression<Func<EMAIL_VERIFICATION, EmailVerificationTransporter>> 
          EmailVerificationProjection =
            emailVerification => new EmailVerificationTransporter
            {
                EmailVerificationId = emailVerification.email_verification_id,
                AccountId = emailVerification.account_id,
                ExpiresAt = emailVerification.expires_at,
                VerifiedAt = emailVerification.verified_at,
                Attempts = emailVerification.attempts,
                IsUsed = emailVerification.is_used,
                CreatedAt = emailVerification.created_at
            };

        private static readonly Expression<Func<EMAIL_VERIFICATION, EmailVerificationTokenTransporter>>   EmailVerificationTokenProjection =
            emailVerification => new EmailVerificationTokenTransporter
            {
                EmailVerificationId = emailVerification.email_verification_id,
                AccountId = emailVerification.account_id,
                VerificationCodeHash = emailVerification.verification_code_hash,
                ExpiresAt = emailVerification.expires_at,
                VerifiedAt = emailVerification.verified_at,
                Attempts = emailVerification.attempts,
                IsUsed = emailVerification.is_used,
                CreatedAt = emailVerification.created_at
            };

        public EmailVerificationRepository(HangmanDBEntities context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<EmailVerificationTransporter> GetByIdAsync(int emailVerificationId)
        {
            return await context.EMAIL_VERIFICATION
                .AsNoTracking()
                .Where(emailVerification => emailVerification.email_verification_id == emailVerificationId)
                .Select(EmailVerificationProjection)
                .FirstOrDefaultAsync();
        }

        public async Task<EmailVerificationTokenTransporter> GetLatestUnusedByAccountIdAsync(int accountId)
        {
            return await context.EMAIL_VERIFICATION
                .AsNoTracking()
                .Where(emailVerification =>
                    emailVerification.account_id == accountId &&
                    !emailVerification.is_used)
                .OrderByDescending(emailVerification => emailVerification.created_at)
                .Select(EmailVerificationTokenProjection)
                .FirstOrDefaultAsync();
        }

        public void Add(CreateEmailVerificationTransporter emailVerification)
        {
            if (emailVerification == null)
            {
                throw new ArgumentNullException(nameof(emailVerification));
            }

            EMAIL_VERIFICATION entity = new EMAIL_VERIFICATION
            {
                account_id = emailVerification.AccountId,
                verification_code_hash = emailVerification.VerificationCodeHash,
                expires_at = emailVerification.ExpiresAt,
                verified_at = null,
                attempts = EmailVerificationDefaults.InitialAttempts,
                is_used = EmailVerificationDefaults.InitialIsUsed,
                created_at = DateTime.UtcNow
            };

            context.EMAIL_VERIFICATION.Add(entity);
        }

        public async Task<bool> MarkAsUsedAsync(int emailVerificationId)
        {
            EMAIL_VERIFICATION emailVerification = await context.EMAIL_VERIFICATION
                .FirstOrDefaultAsync(item => item.email_verification_id == emailVerificationId);

            if (emailVerification == null)
            {
                return false;
            }

            DateTime currentDate = DateTime.UtcNow;

            emailVerification.is_used = true;
            emailVerification.verified_at = currentDate;

            return true;
        }

        public async Task<bool> IncrementAttemptsAsync(int emailVerificationId)
        {
            EMAIL_VERIFICATION emailVerification = await context.EMAIL_VERIFICATION
                .FirstOrDefaultAsync(item => item.email_verification_id == emailVerificationId);

            if (emailVerification == null)
            {
                return false;
            }

            emailVerification.attempts++;

            return true;
        }

        public async Task<int> InvalidateUnusedByAccountIdAsync(int accountId)
        {
            EMAIL_VERIFICATION[] unusedVerifications = await context.EMAIL_VERIFICATION
                .Where(emailVerification =>
                    emailVerification.account_id == accountId &&
                    !emailVerification.is_used)
                .ToArrayAsync();

            foreach (EMAIL_VERIFICATION emailVerification in unusedVerifications)
            {
                emailVerification.is_used = true;
            }

            return unusedVerifications.Length;
        }

        public async Task<int> CleanExpiredUnusedAsync(DateTime expirationLimit)
        {
            EMAIL_VERIFICATION[] expiredVerifications = await context.EMAIL_VERIFICATION
                .Where(emailVerification =>
                    !emailVerification.is_used &&
                    emailVerification.expires_at <= expirationLimit)
                .ToArrayAsync();

            foreach (EMAIL_VERIFICATION emailVerification in expiredVerifications)
            {
                emailVerification.is_used = true;
            }

            return expiredVerifications.Length;
        }
    }
}
