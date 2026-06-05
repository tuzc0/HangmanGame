using Hangman.DataAccess.Interfaces;

namespace Hangman.Business.Factories
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork Create();
    }
}
