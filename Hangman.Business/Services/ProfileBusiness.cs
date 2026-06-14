using Hangman.Business.Configuration;
using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using Hangman.Business.UserCases.Profile;
using Hangman.Contracts.Profile;
using System;
using System.Threading.Tasks;

namespace Hangman.Business.Services
{
    public class ProfileBusiness : IProfileBusiness
    {
        private readonly GetProfileUseCase getProfileUseCase;
        private readonly UpdateProfileUseCase updateProfileUseCase;
        private readonly DeleteProfileUseCase deleteProfileUseCase;

        public ProfileBusiness(IUnitOfWorkFactory unitOfWorkFactory)
        {
            if (unitOfWorkFactory == null)
            {
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
            }

            AuthSettingsProvider authSettingsProvider = new AuthSettingsProvider();

            getProfileUseCase = new GetProfileUseCase(unitOfWorkFactory);

            updateProfileUseCase = new UpdateProfileUseCase(
                unitOfWorkFactory,
                authSettingsProvider);

            deleteProfileUseCase = new DeleteProfileUseCase(unitOfWorkFactory);
        }

        public Task<GetProfileResponse> GetProfileAsync(GetProfileRequest request)
        {
            return getProfileUseCase.ExecuteAsync(request);
        }

        public Task<UpdateProfileResponse> UpdateProfileAsync(
            UpdateProfileRequest request)
        {
            return updateProfileUseCase.ExecuteAsync(request);
        }

        public Task<DeleteProfileResponse> DeleteProfileAsync(
            DeleteProfileRequest request)
        {
            return deleteProfileUseCase.ExecuteAsync(request);
        }
    }
}
