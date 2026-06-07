using Hangman.Contracts.Profile;
using System.Threading.Tasks;

namespace Hangman.Business.Interfaces
{
    public interface IProfileBusiness
    {
        Task<GetProfileResponse> GetProfileAsync(GetProfileRequest request);

        Task<UpdateProfileResponse> UpdateProfileAsync(UpdateProfileRequest request);

        Task<DeleteProfileResponse> DeleteProfileAsync(DeleteProfileRequest request);
    }
}
