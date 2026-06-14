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
        public static Task<PlayerAvailabilityResult> ValidateForProfileAsync(
            IUnitOfWork unitOfWork,
            int accountId)
        {
            return ValidateAsync(
                unitOfWork,
                accountId,
                ProfileMessageCode.AccountNotFound,
                ProfileMessageCode.AccountNotAvailable,
                ProfileMessageCode.EmailVerificationRequired,
                ProfileMessageCode.PlayerProfileNotAvailable);
        }

        public static Task<PlayerAvailabilityResult> ValidateForMatchAsync(
            IUnitOfWork unitOfWork,
            int accountId)
        {
            return ValidateAsync(
                unitOfWork,
                accountId,
                MatchMessageCode.AccountNotFound,
                MatchMessageCode.AccountNotAvailable,
                MatchMessageCode.EmailVerificationRequired,
                MatchMessageCode.PlayerProfileNotAvailable);
        }

        private static async Task<PlayerAvailabilityResult> ValidateAsync(
            IUnitOfWork unitOfWork,
            int accountId,
            Enum accountNotFoundCode,
            Enum accountNotAvailableCode,
            Enum emailVerificationRequiredCode,
            Enum playerProfileNotAvailableCode)
        {
            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            AccountTransporter account = await unitOfWork.Accounts.GetByIdAsync(accountId);

            if (account == null)
            {
                return PlayerAvailabilityResult.Fail(accountNotFoundCode);
            }

            if (account.AccountStatus == AccountStatusConstants.Blocked ||
                account.AccountStatus == AccountStatusConstants.Deleted)
            {
                return PlayerAvailabilityResult.Fail(accountNotAvailableCode);
            }

            if (!account.IsEmailVerified ||
                account.AccountStatus == AccountStatusConstants.PendingVerification)
            {
                return PlayerAvailabilityResult.Fail(emailVerificationRequiredCode);
            }

            if (account.AccountStatus != AccountStatusConstants.Active)
            {
                return PlayerAvailabilityResult.Fail(accountNotAvailableCode);
            }

            PlayerTransporter player = await unitOfWork.Players.GetByIdAsync(account.PlayerId);

            if (player == null || !player.IsActive)
            {
                return PlayerAvailabilityResult.Fail(playerProfileNotAvailableCode);
            }

            return PlayerAvailabilityResult.Success(account, player);
        }
    }
}
