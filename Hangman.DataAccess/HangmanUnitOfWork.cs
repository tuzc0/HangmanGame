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
