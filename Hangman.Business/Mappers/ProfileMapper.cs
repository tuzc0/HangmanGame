using Hangman.Contracts.Profile;
using Hangman.DataAccess.Transporters;

namespace Hangman.Business.Mappers
{
    internal static class ProfileMapper
    {
        public static ProfileDto ToProfileDto(
            AccountTransporter account,
            PlayerTransporter player)
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
    }
}
