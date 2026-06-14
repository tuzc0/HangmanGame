using Hangman.Business.Factories;
using Hangman.Business.Mappers;
using Hangman.Business.Messages;
using Hangman.Business.Policies;
using Hangman.Business.Results;
using Hangman.Business.Validators;
using Hangman.Contracts.Profile;
using System;
using System.Threading.Tasks;

namespace Hangman.Business.UserCases.Profile
{
    internal class GetProfileUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public GetProfileUseCase(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<GetProfileResponse> ExecuteAsync(GetProfileRequest request)
        {
            ValidationResult validationResult = ProfileValidator.ValidateGetProfile(request);

            if (!validationResult.IsValid)
            {
                return ProfileResponseFactory.BuildGetProfileResponse(
                    false,
                    validationResult.MessageCode,
                    null);
            }

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                PlayerAvailabilityResult availabilityResult =
                    await PlayerAvailabilityPolicy.ValidateForProfileAsync(
                        unitOfWork,
                        request.AccountId);

                if (!availabilityResult.IsAvailable)
                {
                    return ProfileResponseFactory.BuildGetProfileResponse(
                        false,
                        availabilityResult.MessageCode,
                        null);
                }

                ProfileDto profile = ProfileMapper.ToProfileDto(
                    availabilityResult.Account,
                    availabilityResult.Player);

                return ProfileResponseFactory.BuildGetProfileResponse(
                    true,
                    ProfileMessageCode.ProfileRetrieved,
                    profile);
            }
        }
    }
}
