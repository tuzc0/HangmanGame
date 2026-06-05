using System;
using System.Threading.Tasks;

namespace Hangman.DataAccess.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IAccountRepository Accounts { get; }

        Task<int> CommitAsync();
    }
}
