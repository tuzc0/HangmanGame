using System;
using System.Threading.Tasks;

namespace Hangman.DataAccess.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IAccountRepository Accounts { get; }

        IEmailVerificationRepository EmailVerifications { get; }

        IPlayerRepository Players { get; }

        IScoreMovementRepository ScoreMovements { get; }

        IWordRepository Words { get; }

        IMatchRepository Matches { get; }

        Task<int> CommitAsync();
    }
}
