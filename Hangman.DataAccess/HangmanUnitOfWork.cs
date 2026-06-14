using Hangman.DataAccess.Interfaces;
using Hangman.DataAccess.Model;
using Hangman.DataAccess.Repositories;
using System;
using System.Threading.Tasks;

namespace Hangman.DataAccess
{
    public class HangmanUnitOfWork : IUnitOfWork
    {
        private readonly HangmanDBEntities context;

        private IAccountRepository accountRepository;

        private IPasswordResetTokenRepository passwordResetTokenRepository;

        private IEmailVerificationRepository emailVerificationRepository;

        private IPlayerRepository playerRepository;

        private IScoreMovementRepository scoreMovementRepository;

        private IWordRepository wordRepository;

        private IMatchRepository matchRepository;

        private IMatchCategoryVoteRepository matchCategoryVoteRepository;

        private bool disposed;

        public HangmanUnitOfWork(HangmanDBEntities context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));

            this.context.Configuration.LazyLoadingEnabled = false;
            this.context.Configuration.ProxyCreationEnabled = false;
        }

        public IAccountRepository Accounts
        {
            get
            {
                if (accountRepository == null)
                {
                    accountRepository = new AccountRepository(context);
                }

                return accountRepository;
            }
        }

        public IPasswordResetTokenRepository PasswordResetTokens
        {
            get
            {
                if (passwordResetTokenRepository == null)
                {
                    passwordResetTokenRepository = new PasswordResetTokenRepository(context);
                }

                return passwordResetTokenRepository;
            }
        }

        public IEmailVerificationRepository EmailVerifications
        {
            get
            {
                if (emailVerificationRepository == null)
                {
                    emailVerificationRepository = new EmailVerificationRepository(context);
                }

                return emailVerificationRepository;
            }
        }

        public IPlayerRepository Players
        {
            get
            {
                if (playerRepository == null)
                {
                    playerRepository = new PlayerRepository(context);
                }

                return playerRepository;
            }
        }

        public IScoreMovementRepository ScoreMovements
        {
            get
            {
                if (scoreMovementRepository == null)
                {
                    scoreMovementRepository = new ScoreMovementRepository(context);
                }

                return scoreMovementRepository;
            }
        }

        public IWordRepository Words
        {
            get
            {
                if (wordRepository == null)
                {
                    wordRepository = new WordRepository(context);
                }

                return wordRepository;
            }
        }

        public IMatchRepository Matches
        {
            get
            {
                if (matchRepository == null)
                {
                    matchRepository = new MatchRepository(context);
                }

                return matchRepository;
            }
        }

        public IMatchCategoryVoteRepository MatchCategoryVotes
        {
            get
            {
                if (matchCategoryVoteRepository == null)
                {
                    matchCategoryVoteRepository = new MatchCategoryVoteRepository(context);
                }

                return matchCategoryVoteRepository;
            }
        }

        public async Task<int> CommitAsync()
        {
            return await context.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                context.Dispose();
            }

            disposed = true;
        }
    }
}
