using Hangman.Contracts.Auth;
using System;

namespace Hangman.Business.Mappers
{
    public static class AuthResponseFactory
    {
        public static RegisterResponse BuildRegisterResponse(
            bool success,
            Enum messageCode,
            int accountId,
            int playerId,
            bool requiresEmailVerification,
            bool verificationEmailSent)
        {
            return new RegisterResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                AccountId = accountId,
                PlayerId = playerId,
                RequiresEmailVerification = requiresEmailVerification,
                VerificationEmailSent = verificationEmailSent
            };
        }

        public static LoginResponse BuildLoginResponse(
            bool success,
            Enum messageCode,
            AuthenticatedPlayerDto player)
        {
            return new LoginResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Player = player
            };
        }

        public static ResendVerificationEmailResponse BuildResendVerificationEmailResponse(
            bool success,
            Enum messageCode,
            bool verificationEmailSent)
        {
            return new ResendVerificationEmailResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                VerificationEmailSent = verificationEmailSent
            };
        }

        public static RequestPasswordResetResponse BuildRequestPasswordResetResponse(
            bool success,
            Enum messageCode)
        {
            return new RequestPasswordResetResponse
            {
                Success = success,
                MessageCode = messageCode.ToString()
            };
        }

        public static ResetPasswordResponse BuildResetPasswordResponse(
            bool success,
            Enum messageCode)
        {
            return new ResetPasswordResponse
            {
                Success = success,
                MessageCode = messageCode.ToString()
            };
        }

        public static VerifyEmailResponse BuildVerifyEmailResponse(
            bool success,
            Enum messageCode,
            int accountId,
            bool isEmailVerified)
        {
            return new VerifyEmailResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                AccountId = accountId,
                IsEmailVerified = isEmailVerified
            };
        }
    }
}
