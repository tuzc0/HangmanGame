using Hangman.Business.Constants;
using Hangman.DataAccess.Transporters;

namespace Hangman.Business.Policies
{
    public static class AccountAvailabilityPolicy
    {
        public static bool IsAvailableForPasswordReset(AccountTransporter account)
        {
            if (account == null)
            {
                return false;
            }

            if (account.AccountStatus == AccountStatusConstants.Blocked ||
                account.AccountStatus == AccountStatusConstants.Deleted)
            {
                return false;
            }

            if (!account.IsEmailVerified ||
                account.AccountStatus == AccountStatusConstants.PendingVerification)
            {
                return false;
            }

            return account.AccountStatus == AccountStatusConstants.Active;
        }

        public static bool IsBlockedOrDeleted(string accountStatus)
        {
            return accountStatus == AccountStatusConstants.Blocked ||
                   accountStatus == AccountStatusConstants.Deleted;
        }

        public static bool RequiresEmailVerification(bool isEmailVerified, string accountStatus)
        {
            return !isEmailVerified ||
                   accountStatus == AccountStatusConstants.PendingVerification;
        }

        public static bool IsActive(string accountStatus)
        {
            return accountStatus == AccountStatusConstants.Active;
        }
    }
}
