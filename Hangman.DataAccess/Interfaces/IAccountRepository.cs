using Hangman.DataAccess.Transporters;
using System.Threading.Tasks;

namespace Hangman.DataAccess.Interfaces
{
    public interface IAccountRepository
    {
        Task<AccountTransporter> GetByIdAsync(int accountId);

        Task<AccountTransporter> GetByPlayerIdAsync(int playerId);

        Task<AccountTransporter> GetByEmailAsync(string email);

        Task<AccountCredentialsTransporter> GetCredentialsByEmailAsync(string email);

        Task<bool> EmailExistsAsync(string email);

        void Add(CreateAccountTransporter account);

        void AddPendingAccount(CreatePendingAccountTransporter registration);

        Task<bool> MarkEmailAsVerifiedAsync(MarkEmailAsVerifiedTransporter verification);

        Task<bool> UpdatePasswordHashAsync(int accountId, string passwordHash);
    }
}
