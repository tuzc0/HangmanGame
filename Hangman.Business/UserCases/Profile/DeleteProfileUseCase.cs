using Hangman.Business.Constants;
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
    internal class DeleteProfileUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public DeleteProfileUseCase(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<DeleteProfileResponse> ExecuteAsync(DeleteProfileRequest request)
        {
            ValidationResult validationResult = ProfileValidator.ValidateDeleteProfile(request);

            if (!validationResult.IsValid)
            {
                return ProfileResponseFactory.BuildDeleteProfileResponse(
                    false,
                    validationResult.MessageCode);
            }

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                PlayerAvailabilityResult availabilityResult =
                    await PlayerAvailabilityPolicy.ValidateForProfileAsync(
                        unitOfWork,
                        request.AccountId);

                if (!availabilityResult.IsAvailable)
                {
                    return ProfileResponseFactory.BuildDeleteProfileResponse(
                        false,
                        availabilityResult.MessageCode);
                }

                bool accountUpdated = await unitOfWork.Accounts.UpdateStatusAsync(
                    availabilityResult.Account.AccountId,
                    AccountStatusConstants.Deleted);

                bool playerUpdated = await unitOfWork.Players.UpdateActiveStatusAsync(
                    new UpdatePlayerActiveStatusTransporter
                    {
                        PlayerId = availabilityResult.Player.PlayerId,
                        IsActive = false
                    });

                if (!accountUpdated || !playerUpdated)
                {
                    return ProfileResponseFactory.BuildDeleteProfileResponse(
                        false,
                        ProfileMessageCode.ProfileDeleteFailed);
                }

                await unitOfWork.CommitAsync();

                return ProfileResponseFactory.BuildDeleteProfileResponse(
                    true,
                    ProfileMessageCode.ProfileDeleted);
            }
        }
    }
}
