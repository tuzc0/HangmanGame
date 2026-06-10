using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using Hangman.Business.Messages;
using Hangman.Business.Services;
using Hangman.Contracts.Contracts;
using Hangman.Contracts.Match;
using HangmanGame.Services.ExceptionHandling;
using log4net;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace HangmanGame.Services.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class MatchService : IMatchService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MatchService));

        private readonly IMatchBusiness matchBusiness;

        public MatchService()
            : this(new MatchBusiness(new UnitOfWorkFactory()))
        {
        }

        internal MatchService(IMatchBusiness matchBusiness)
        {
            this.matchBusiness = matchBusiness ??
                throw new ArgumentNullException(nameof(matchBusiness));
        }

        public async Task<CreateLobbyResponse> CreateLobbyAsync(CreateLobbyRequest request)
        {
            try
            {
                return await matchBusiness.CreateLobbyAsync(request);
            }
            catch (Exception exception)
            {
                MatchMessageCode messageCode = ServiceExceptionMapper.MapMatch(exception);

                Log.ErrorFormat(
                    "Error executing CreateLobbyAsync. MessageCode: {0}. HostAccountId: {1}",
                    messageCode,
                    request != null ? request.HostAccountId : 0,
                    exception);

                return new CreateLobbyResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString(),
                    Lobby = null
                };
            }
        }

        public async Task<GetAvailableLobbiesResponse> GetAvailableLobbiesAsync(
            GetAvailableLobbiesRequest request)
        {
            try
            {
                return await matchBusiness.GetAvailableLobbiesAsync(request);
            }
            catch (Exception exception)
            {
                MatchMessageCode messageCode = ServiceExceptionMapper.MapMatch(exception);

                Log.ErrorFormat(
                    "Error executing GetAvailableLobbiesAsync. MessageCode: {0}. AccountId: {1}",
                    messageCode,
                    request != null ? request.AccountId : 0,
                    exception);

                return new GetAvailableLobbiesResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString(),
                    Lobbies = new List<AvailableLobbyDto>()
                };
            }
        }

        public async Task<JoinLobbyResponse> JoinLobbyAsync(JoinLobbyRequest request)
        {
            try
            {
                return await matchBusiness.JoinLobbyAsync(request);
            }
            catch (Exception exception)
            {
                MatchMessageCode messageCode = ServiceExceptionMapper.MapMatch(exception);

                Log.ErrorFormat(
                    "Error executing JoinLobbyAsync. MessageCode: {0}. MatchId: {1}. GuestAccountId: {2}",
                    messageCode,
                    request != null ? request.MatchId : 0,
                    request != null ? request.GuestAccountId : 0,
                    exception);

                return new JoinLobbyResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString(),
                    Lobby = null
                };
            }
        }
    }
}