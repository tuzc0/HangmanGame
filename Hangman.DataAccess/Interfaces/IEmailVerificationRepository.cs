using Hangman.DataAccess.Transporters;
using System;
using System.Threading.Tasks;

namespace Hangman.DataAccess.Interfaces
{
    public interface IEmailVerificationRepository
    {
        Task<EmailVerificationTransporter> GetByIdAsync(int emailVerificationId);

        Task<EmailVerificationTokenTransporter> GetLatestUnusedByAccountIdAsync(int accountId);

        void Add(CreateEmailVerificationTransporter emailVerification);

        Task<bool> MarkAsUsedAsync(int emailVerificationId);

        Task<bool> IncrementAttemptsAsync(int emailVerificationId);

        Task<int> InvalidateUnusedByAccountIdAsync(int accountId);

        Task<int> CleanExpiredUnusedAsync(DateTime expirationLimit);
    }
}
