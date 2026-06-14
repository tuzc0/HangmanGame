using Hangman.Business.Constants;
using Hangman.Business.Messages;
using Hangman.Business.Results;
using Hangman.DataAccess.Interfaces;
using Hangman.DataAccess.Transporters;
using System;
using System.Threading.Tasks;

namespace Hangman.Business.Policies
{
    internal static class PlayerAvailabilityPolicy
    {
        public static async Task<PlayerAvailabilityResult> ValidateForProfileAsync(
            IUnitOfWork unitOfWork,
            int accountId)
        {
            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            AccountTransporter account = await unitOfWork.Accounts.GetByIdAsync(accountId);

            return await ValidateForProfileAsync(unitOfWork, account);
        }

        private static async Task<PlayerAvailabilityResult> ValidateForProfileAsync(
            IUnitOfWork unitOfWork,
            AccountTransporter account)
        {
            if (account == null)
            {
                return PlayerAvailabilityResult.Fail(ProfileMessageCode.AccountNotFound);
            }

            if (account.AccountStatus == AccountStatusConstants.Blocked ||
                account.AccountStatus == AccountStatusConstants.Deleted)
            {
                return PlayerAvailabilityResult.Fail(ProfileMessageCode.AccountNotAvailable);
            }

            if (!account.IsEmailVerified ||
                account.AccountStatus == AccountStatusConstants.PendingVerification)
            {
                return PlayerAvailabilityResult.Fail(
                    ProfileMessageCode.EmailVerificationRequired);
            }

            if (account.AccountStatus != AccountStatusConstants.Active)
            {
                return PlayerAvailabilityResult.Fail(ProfileMessageCode.AccountNotAvailable);
            }

            PlayerTransporter player = await unitOfWork.Players.GetByIdAsync(account.PlayerId);

            if (player == null || !player.IsActive)
            {
                return PlayerAvailabilityResult.Fail(
                    ProfileMessageCode.PlayerProfileNotAvailable);
            }

            return PlayerAvailabilityResult.Success(account, player);
        }
    }
}
