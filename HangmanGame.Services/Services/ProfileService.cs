using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using Hangman.Business.Messages;
using Hangman.Business.Services;
using Hangman.Contracts.Contracts;
using Hangman.Contracts.Profile;
using HangmanGame.Services.ExceptionHandling;
using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace HangmanGame.Services.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class ProfileService : IProfileService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProfileService));

        private readonly IProfileBusiness profileBusiness;

        public ProfileService()
            : this(new ProfileBusiness(new UnitOfWorkFactory()))
        {
        }

        internal ProfileService(IProfileBusiness profileBusiness)
        {
            this.profileBusiness = profileBusiness ?? throw new ArgumentNullException(nameof(profileBusiness));
        }

        public async Task<GetProfileResponse> GetProfileAsync(GetProfileRequest request)
        {
            try
            {
                return await profileBusiness.GetProfileAsync(request);
            }
            catch (Exception exception)
            {
                ProfileMessageCode messageCode = ServiceExceptionMapper.MapProfile(exception);

                Log.Error(
                    string.Format("Error executing GetProfileAsync. MessageCode: {0}. AccountId: {1}",
                    messageCode,
                    request != null ? request.AccountId : 0),
                    exception);

                return new GetProfileResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString(),
                    Profile = null
                };
            }
        }

        public async Task<UpdateProfileResponse> UpdateProfileAsync(UpdateProfileRequest request)
        {
            try
            {
                return await profileBusiness.UpdateProfileAsync(request);
            }
            catch (Exception exception)
            {
                ProfileMessageCode messageCode = ServiceExceptionMapper.MapProfile(exception);

                Log.Error(
                    string.Format("Error executing UpdateProfileAsync. MessageCode: {0}. AccountId: {1}",
                    messageCode,
                    request != null ? request.AccountId : 0),
                    exception);

                return new UpdateProfileResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString(),
                    Profile = null
                };
            }
        }

        public async Task<DeleteProfileResponse> DeleteProfileAsync(DeleteProfileRequest request)
        {
            try
            {
                return await profileBusiness.DeleteProfileAsync(request);
            }
            catch (Exception exception)
            {
                ProfileMessageCode messageCode = ServiceExceptionMapper.MapProfile(exception);

                Log.Error(
                    string.Format("Error executing DeleteProfileAsync. MessageCode: {0}. AccountId: {1}",
                    messageCode,
                    request != null ? request.AccountId : 0),
                    exception);

                return new DeleteProfileResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString()
                };
            }
        }
    }
}