using Hangman.Contracts.Profile;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Hangman.Contracts.Contracts
{
    [ServiceContract]
    public interface IProfileService
    {
        [OperationContract]
        Task<GetProfileResponse> GetProfileAsync(GetProfileRequest request);

        [OperationContract]
        Task<UpdateProfileResponse> UpdateProfileAsync(UpdateProfileRequest request);

        [OperationContract]
        Task<DeleteProfileResponse> DeleteProfileAsync(DeleteProfileRequest request);
    }
}
