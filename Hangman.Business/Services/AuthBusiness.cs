using Hangman.Business.Configuration;
using Hangman.Business.Email;
using Hangman.Business.Factories;
using Hangman.Business.Helpers;
using Hangman.Business.Interfaces;
using Hangman.Business.UserCases.Auth;
using Hangman.Contracts.Auth;
using System;
using System.Threading.Tasks;

namespace Hangman.Business.Services
{
    public class AuthBusiness : IAuthBusiness
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly RegisterUseCase registerUseCase;
        private readonly LoginUseCase loginUseCase;
        private readonly ResendVerificationEmailUseCase resendVerificationEmailUseCase;
        private readonly RequestPasswordResetUseCase requestPasswordResetUseCase;
        private readonly ResetPasswordUseCase resetPasswordUseCase;

        public AuthBusiness(IUnitOfWorkFactory unitOfWorkFactory, IEmailSender emailSender)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

            if (emailSender == null)
            {
                throw new ArgumentNullException(nameof(emailSender));
            }

            AuthSettingsProvider authSettingsProvider = new AuthSettingsProvider();

            registerUseCase = new RegisterUseCase(
                unitOfWorkFactory,
                emailSender,
                authSettingsProvider);

            loginUseCase = new LoginUseCase(unitOfWorkFactory);

            resendVerificationEmailUseCase = new ResendVerificationEmailUseCase(
                unitOfWorkFactory,
                emailSender,
                authSettingsProvider);

            requestPasswordResetUseCase = new RequestPasswordResetUseCase(
                unitOfWorkFactory,
                emailSender,
                authSettingsProvider);

            resetPasswordUseCase = new ResetPasswordUseCase(
                unitOfWorkFactory,
                authSettingsProvider);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                return await unitOfWork.Accounts.EmailExistsAsync(EmailNormalizer.Normalize(email));
            }
        }

        public Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            return registerUseCase.ExecuteAsync(request);
        }

        public Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            return loginUseCase.ExecuteAsync(request);
        }

        public Task<ResendVerificationEmailResponse> ResendVerificationEmailAsync(
            ResendVerificationEmailRequest request)
        {
            return resendVerificationEmailUseCase.ExecuteAsync(request);
        }

        public Task<RequestPasswordResetResponse> RequestPasswordResetAsync(
            RequestPasswordResetRequest request)
        {
            return requestPasswordResetUseCase.ExecuteAsync(request);
        }

        public Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request)
        {
            return resetPasswordUseCase.ExecuteAsync(request);
        }
    }
}
