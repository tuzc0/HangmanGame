using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using Hangman.Business.Messages;
using Hangman.Business.Services;
using Hangman.Contracts.Auth;
using Hangman.Contracts.Contracts;
using Hangman.Infrastructure.Email;
using HangmanGame.Services.ExceptionHandling;
using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace HangmanGame.Services.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class AuthService : IAuthService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AuthService));

        private readonly IAuthBusiness authBusiness;

        public AuthService()
            : this(new AuthBusiness(new UnitOfWorkFactory(), new SmtpEmailSender()))
        {
        }

        internal AuthService(IAuthBusiness authBusiness)
        {
            this.authBusiness = authBusiness ?? throw new ArgumentNullException(nameof(authBusiness));
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                return await authBusiness.RegisterAsync(request);
            }
            catch (Exception exception)
            {
                AuthMessageCode messageCode = ServiceExceptionMapper.Map(exception);

                Log.ErrorFormat("Error executing RegisterAsync. MessageCode: {0}. Email: {1}",
                    messageCode,
                    request != null ? request.Email : "null",
                    exception);

                return new RegisterResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString(),
                    AccountId = 0,
                    PlayerId = 0,
                    RequiresEmailVerification = false,
                    VerificationEmailSent = false
                };
            }
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                return await authBusiness.LoginAsync(request);
            }
            catch (Exception exception)
            {
                AuthMessageCode messageCode = ServiceExceptionMapper.Map(exception);

                Log.ErrorFormat("Error executing LoginAsync. MessageCode: {0}. Email: {1}",
                    messageCode,
                    request != null ? request.Email : "null",
                    exception);

                return new LoginResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString(),
                    Player = null
                };
            }
        }

        public async Task<ResendVerificationEmailResponse> ResendVerificationEmailAsync(
            ResendVerificationEmailRequest request)
        {
            try
            {
                return await authBusiness.ResendVerificationEmailAsync(request);
            }
            catch (Exception exception)
            {
                AuthMessageCode messageCode = ServiceExceptionMapper.Map(exception);

                Log.ErrorFormat("Error executing ResendVerificationEmailAsync. MessageCode: {0}. Email: {1}",
                    messageCode,
                    request != null ? request.Email : "null",
                    exception);

                return new ResendVerificationEmailResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString(),
                    VerificationEmailSent = false
                };
            }
        }
    }
}