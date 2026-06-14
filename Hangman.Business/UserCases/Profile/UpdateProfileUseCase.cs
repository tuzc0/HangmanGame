using Hangman.Business.Configuration;
using Hangman.Business.Factories;
using Hangman.Business.Mappers;
using Hangman.Business.Messages;
using Hangman.Business.Policies;
using Hangman.Business.Results;
using Hangman.Business.Validators;
using Hangman.Contracts.Profile;
using Hangman.DataAccess.Transporters;
using System;
using System.Threading.Tasks;

namespace Hangman.Business.UserCases.Profile
{
    internal class UpdateProfileUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly AuthSettingsProvider authSettingsProvider;

        public UpdateProfileUseCase(
            IUnitOfWorkFactory unitOfWorkFactory,
            AuthSettingsProvider authSettingsProvider)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
            this.authSettingsProvider = authSettingsProvider ??
                throw new ArgumentNullException(nameof(authSettingsProvider));
        }

        public async Task<UpdateProfileResponse> ExecuteAsync(UpdateProfileRequest request)
        {
            AuthSettings settings = authSettingsProvider.GetSettings();
            ProfileValidator validator = new ProfileValidator(settings);

            ValidationResult validationResult = validator.ValidateUpdateProfile(request);

            if (!validationResult.IsValid)
            {
                return ProfileResponseFactory.BuildUpdateProfileResponse(
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
                    return ProfileResponseFactory.BuildUpdateProfileResponse(
                        false,
                        availabilityResult.MessageCode,
                        null);
                }

                bool updated = await unitOfWork.Players.UpdateProfileAsync(
                    new UpdatePlayerProfileTransporter
                    {
                        PlayerId = availabilityResult.Player.PlayerId,
                        FullName = request.FullName.Trim(),
                        DateOfBirth = request.DateOfBirth,
                        Phone = request.Phone.Trim(),
                        PreferredLanguageCode = request.PreferredLanguageCode
                            .Trim()
                            .ToLowerInvariant()
                    });

                if (!updated)
                {
                    return ProfileResponseFactory.BuildUpdateProfileResponse(
                        false,
                        ProfileMessageCode.ProfileUpdateFailed,
                        null);
                }

                await unitOfWork.CommitAsync();

                PlayerTransporter updatedPlayer =
                    await unitOfWork.Players.GetByIdAsync(
                        availabilityResult.Player.PlayerId);

                ProfileDto profile = ProfileMapper.ToProfileDto(
                    availabilityResult.Account,
                    updatedPlayer);

                return ProfileResponseFactory.BuildUpdateProfileResponse(
                    true,
                    ProfileMessageCode.ProfileUpdated,
                    profile);
            }
        }
    }
}
