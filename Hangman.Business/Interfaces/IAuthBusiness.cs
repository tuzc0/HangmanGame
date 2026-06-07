using Hangman.Contracts.Auth;
using System.Threading.Tasks;

namespace Hangman.Business.Interfaces
{
    public interface IAuthBusiness
    {
        Task<bool> EmailExistsAsync(string email);

        Task<RegisterResponse> RegisterAsync(RegisterRequest request);

        Task<LoginResponse> LoginAsync(LoginRequest request);

        Task<ResendVerificationEmailResponse> ResendVerificationEmailAsync(
            ResendVerificationEmailRequest request);
    }
}
