using Hangman.Business.Factories;
using Hangman.Business.Helpers;
using Hangman.Business.Mappers;
using Hangman.Business.Messages;
using Hangman.Business.Policies;
using Hangman.Business.Results;
using Hangman.Business.Security;
using Hangman.Business.Validators;
using Hangman.Contracts.Auth;
using Hangman.DataAccess.Transporters;
using System;
using System.Threading.Tasks;

namespace Hangman.Business.UserCases.Auth
{
    internal class LoginUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public LoginUseCase(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<LoginResponse> ExecuteAsync(LoginRequest request)
        {
            ValidationResult validationResult = AuthValidator.ValidateLogin(request);

            if (!validationResult.IsValid)
            {
                return AuthResponseFactory.BuildLoginResponse(false, validationResult.MessageCode, null);
            }

            string email = EmailNormalizer.Normalize(request.Email);

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                AccountCredentialsTransporter credentials = await unitOfWork.Accounts.GetCredentialsByEmailAsync(email);

                if (credentials == null)
                {
                    return AuthResponseFactory.BuildLoginResponse(
                        false,
                        AuthMessageCode.InvalidEmailOrPassword,
                        null);
                }

                bool passwordIsValid = PasswordHasher.VerifyPassword(request.Password, credentials.PasswordHash);

                if (!passwordIsValid)
                {
                    return AuthResponseFactory.BuildLoginResponse(
                        false,
                        AuthMessageCode.InvalidEmailOrPassword,
                        null);
                }

                if (AccountAvailabilityPolicy.IsBlockedOrDeleted(credentials.AccountStatus))
                {
                    return AuthResponseFactory.BuildLoginResponse(
                        false,
                        AuthMessageCode.AccountNotAvailable,
                        null);
                }

                if (AccountAvailabilityPolicy.RequiresEmailVerification(
                    credentials.IsEmailVerified,
                    credentials.AccountStatus))
                {
                    return AuthResponseFactory.BuildLoginResponse(
                        false,
                        AuthMessageCode.EmailVerificationRequired,
                        null);
                }

                if (!AccountAvailabilityPolicy.IsActive(credentials.AccountStatus))
                {
                    return AuthResponseFactory.BuildLoginResponse(
                        false,
                        AuthMessageCode.AccountNotActive,
                        null);
                }

                PlayerTransporter player = await unitOfWork.Players.GetByAccountIdAsync(credentials.AccountId);

                if (player == null || !player.IsActive)
                {
                    return AuthResponseFactory.BuildLoginResponse(
                        false,
                        AuthMessageCode.PlayerProfileNotAvailable,
                        null);
                }

                AuthenticatedPlayerDto authenticatedPlayer = new AuthenticatedPlayerDto
                {
                    AccountId = credentials.AccountId,
                    PlayerId = player.PlayerId,
                    FullName = player.FullName,
                    DateOfBirth = player.DateOfBirth,
                    Phone = player.Phone,
                    Email = credentials.Email,
                    PreferredLanguageCode = player.PreferredLanguageCode
                };

                return AuthResponseFactory.BuildLoginResponse(
                    true,
                    AuthMessageCode.LoginSuccessful,
                    authenticatedPlayer);
            }
        }
    }
}
