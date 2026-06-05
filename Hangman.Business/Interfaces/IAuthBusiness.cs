using System.Threading.Tasks;

namespace Hangman.Business.Interfaces
{
    public interface IAuthBusiness
    {
        Task<bool> EmailExistsAsync(string email);
    }
}
