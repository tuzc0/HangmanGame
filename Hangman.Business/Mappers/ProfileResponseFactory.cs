using Hangman.Contracts.Profile;
using System;

namespace Hangman.Business.Mappers
{
    internal static class ProfileResponseFactory
    {
        public static GetProfileResponse BuildGetProfileResponse(
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

        public static UpdateProfileResponse BuildUpdateProfileResponse(
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

        public static DeleteProfileResponse BuildDeleteProfileResponse(
            bool success,
            Enum messageCode)
        {
            return new DeleteProfileResponse
            {
                Success = success,
                MessageCode = messageCode.ToString()
            };
        }
    }
}
