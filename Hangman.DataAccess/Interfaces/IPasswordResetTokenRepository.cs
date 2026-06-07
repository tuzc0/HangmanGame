using Hangman.DataAccess.Transporters;
using System;
using System.Threading.Tasks;

namespace Hangman.DataAccess.Interfaces
{
    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetTokenTransporter> GetLatestUnusedByAccountIdAsync(int accountId);

        void Add(CreatePasswordResetTokenTransporter passwordResetToken);

        Task<bool> MarkAsUsedAsync(int passwordResetTokenId);

        Task<bool> IncrementAttemptsAsync(int passwordResetTokenId);

        Task<int> InvalidateUnusedByAccountIdAsync(int accountId);

        Task<int> CleanExpiredUnusedAsync(DateTime expirationLimit);
    }
}
