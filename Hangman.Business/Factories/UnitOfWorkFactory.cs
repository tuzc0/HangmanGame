using Hangman.DataAccess;
using Hangman.DataAccess.Interfaces;
using Hangman.DataAccess.Model;

namespace Hangman.Business.Factories
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        public IUnitOfWork Create()
        {
            return new HangmanUnitOfWork(new HangmanDBEntities());
        }
    }
}
