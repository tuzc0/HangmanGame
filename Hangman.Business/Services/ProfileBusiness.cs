using Hangman.Business.Configuration;
using Hangman.Business.Constants;
using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using Hangman.Business.Messages;
using Hangman.Business.Results;
using Hangman.Business.Validators;
using Hangman.Contracts.Profile;
using Hangman.DataAccess.Transporters;
using System;
using System.Threading.Tasks;

namespace Hangman.Business.Services
{
    public class ProfileBusiness : IProfileBusiness
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly AuthSettingsProvider authSettingsProvider;

        public ProfileBusiness(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ?? 
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
            authSettingsProvider = new AuthSettingsProvider();
        }

        public async Task<GetProfileResponse> GetProfileAsync(GetProfileRequest request)
        {
            ValidationResult validationResult = ProfileValidator.ValidateGetProfile(request);

            if (!validationResult.IsValid)
            {
                return BuildGetProfileResponse(false, validationResult.MessageCode, null);
            }

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                AccountTransporter account = await unitOfWork.Accounts.GetByIdAsync(request.AccountId);

                ProfileAvailabilityResult availabilityResult = await ValidateProfileAvailabilityAsync(
                    unitOfWork,
                    account);

                if (!availabilityResult.IsAvailable)
                {
                    return BuildGetProfileResponse(false, availabilityResult.MessageCode, null);
                }

                ProfileDto profile = BuildProfileDto(account, availabilityResult.Player);

                return BuildGetProfileResponse(true, ProfileMessageCode.ProfileRetrieved, profile);
            }
        }

        public async Task<UpdateProfileResponse> UpdateProfileAsync(UpdateProfileRequest request)
        {
            AuthSettings settings = authSettingsProvider.GetSettings();
            ProfileValidator validator = new ProfileValidator(settings);

            ValidationResult validationResult = validator.ValidateUpdateProfile(request);

            if (!validationResult.IsValid)
            {
                return BuildUpdateProfileResponse(false, validationResult.MessageCode, null);
            }

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                AccountTransporter account = await unitOfWork.Accounts.GetByIdAsync(request.AccountId);

                ProfileAvailabilityResult availabilityResult = await ValidateProfileAvailabilityAsync(
                    unitOfWork,
                    account);

                if (!availabilityResult.IsAvailable)
                {
                    return BuildUpdateProfileResponse(false, availabilityResult.MessageCode, null);
                }

                bool updated = await unitOfWork.Players.UpdateProfileAsync(
                    new UpdatePlayerProfileTransporter
                    {
                        PlayerId = account.PlayerId,
                        FullName = request.FullName.Trim(),
                        DateOfBirth = request.DateOfBirth,
                        Phone = request.Phone.Trim(),
                        PreferredLanguageCode = request.PreferredLanguageCode.Trim().ToLowerInvariant()
                    });

                if (!updated)
                {
                    return BuildUpdateProfileResponse(false, ProfileMessageCode.ProfileUpdateFailed, null);
                }

                await unitOfWork.CommitAsync();

                PlayerTransporter updatedPlayer = await unitOfWork.Players.GetByIdAsync(account.PlayerId);
                ProfileDto profile = BuildProfileDto(account, updatedPlayer);

                return BuildUpdateProfileResponse(true, ProfileMessageCode.ProfileUpdated, profile);
            }
        }

        public async Task<DeleteProfileResponse> DeleteProfileAsync(DeleteProfileRequest request)
        {
            ValidationResult validationResult = ProfileValidator.ValidateDeleteProfile(request);

            if (!validationResult.IsValid)
            {
                return BuildDeleteProfileResponse(false, validationResult.MessageCode);
            }

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                AccountTransporter account = await unitOfWork.Accounts.GetByIdAsync(request.AccountId);

                ProfileAvailabilityResult availabilityResult = await ValidateProfileAvailabilityAsync(
                    unitOfWork,
                    account);

                if (!availabilityResult.IsAvailable)
                {
                    return BuildDeleteProfileResponse(false, availabilityResult.MessageCode);
                }

                bool accountUpdated = await unitOfWork.Accounts.UpdateStatusAsync(
                    account.AccountId,
                    AccountStatusConstants.Deleted);

                bool playerUpdated = await unitOfWork.Players.UpdateActiveStatusAsync(
                    new UpdatePlayerActiveStatusTransporter
                    {
                        PlayerId = account.PlayerId,
                        IsActive = false
                    });

                if (!accountUpdated || !playerUpdated)
                {
                    return BuildDeleteProfileResponse(false, ProfileMessageCode.ProfileDeleteFailed);
                }

                await unitOfWork.CommitAsync();

                return BuildDeleteProfileResponse(true, ProfileMessageCode.ProfileDeleted);
            }
        }

        private static async Task<ProfileAvailabilityResult> ValidateProfileAvailabilityAsync(
            DataAccess.Interfaces.IUnitOfWork unitOfWork,
            AccountTransporter account)
        {
            if (account == null)
            {
                return ProfileAvailabilityResult.Fail(ProfileMessageCode.AccountNotFound);
            }

            if (account.AccountStatus == AccountStatusConstants.Blocked ||
                account.AccountStatus == AccountStatusConstants.Deleted)
            {
                return ProfileAvailabilityResult.Fail(ProfileMessageCode.AccountNotAvailable);
            }

            if (!account.IsEmailVerified ||
                account.AccountStatus == AccountStatusConstants.PendingVerification)
            {
                return ProfileAvailabilityResult.Fail(ProfileMessageCode.EmailVerificationRequired);
            }

            if (account.AccountStatus != AccountStatusConstants.Active)
            {
                return ProfileAvailabilityResult.Fail(ProfileMessageCode.AccountNotAvailable);
            }

            PlayerTransporter player = await unitOfWork.Players.GetByIdAsync(account.PlayerId);

            if (player == null || !player.IsActive)
            {
                return ProfileAvailabilityResult.Fail(ProfileMessageCode.PlayerProfileNotAvailable);
            }

            return ProfileAvailabilityResult.Success(player);
        }

        private static ProfileDto BuildProfileDto(AccountTransporter account, PlayerTransporter player)
        {
            if (account == null || player == null)
            {
                return null;
            }

            return new ProfileDto
            {
                AccountId = account.AccountId,
                PlayerId = player.PlayerId,
                FullName = player.FullName,
                DateOfBirth = player.DateOfBirth,
                Phone = player.Phone,
                Email = account.Email,
                PreferredLanguageCode = player.PreferredLanguageCode
            };
        }

        private static GetProfileResponse BuildGetProfileResponse(
            bool success,
            Enum messageCode,
            ProfileDto profile)
        {
            return new GetProfileResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Profile = profile
            };
        }

        private static UpdateProfileResponse BuildUpdateProfileResponse(
            bool success,
            Enum messageCode,
            ProfileDto profile)
        {
            return new UpdateProfileResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Profile = profile
            };
        }

        private static DeleteProfileResponse BuildDeleteProfileResponse(
            bool success,
            Enum messageCode)
        {
            return new DeleteProfileResponse
            {
                Success = success,
                MessageCode = messageCode.ToString()
            };
        }

        private sealed class ProfileAvailabilityResult
        {
            public bool IsAvailable { get; private set; }

            public ProfileMessageCode MessageCode { get; private set; }

            public PlayerTransporter Player { get; private set; }

            public static ProfileAvailabilityResult Success(PlayerTransporter player)
            {
                return new ProfileAvailabilityResult
                {
                    IsAvailable = true,
                    MessageCode = ProfileMessageCode.ProfileRetrieved,
                    Player = player
                };
            }

            public static ProfileAvailabilityResult Fail(ProfileMessageCode messageCode)
            {
                return new ProfileAvailabilityResult
                {
                    IsAvailable = false,
                    MessageCode = messageCode,
                    Player = null
                };
            }
        }
    }
}
