using Hangman.Contracts.Auth;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Contracts.Contracts
{
    [ServiceContract]
    public interface IAuthService
    {
        [OperationContract]
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);

        [OperationContract]
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}
