using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using Hangman.Business.Messages;
using Hangman.Business.Services;
using Hangman.Contracts.Contracts;
using Hangman.Contracts.Match;
using HangmanGame.Services.ExceptionHandling;
using HangmanGame.Services.Notifications;
using log4net;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace HangmanGame.Services.Services
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.PerCall,
        ConcurrencyMode = ConcurrencyMode.Single)]
    public class MatchGuessService : IMatchGuessService
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(typeof(MatchGuessService));

        private readonly IMatchGuessBusiness matchGuessBusiness;

        public MatchGuessService()
            : this(new MatchGuessBusiness(new UnitOfWorkFactory()))
        {
        }

        internal MatchGuessService(IMatchGuessBusiness matchGuessBusiness)
        {
            this.matchGuessBusiness = matchGuessBusiness ??
                throw new ArgumentNullException(nameof(matchGuessBusiness));
        }

        public async Task<GetMatchGameStateResponse> GetGameStateAsync(
            GetMatchGameStateRequest request)
        {
            try
            {
                return await matchGuessBusiness.GetGameStateAsync(request);
            }
            catch (Exception exception)
            {
                MatchMessageCode messageCode =
                    ServiceExceptionMapper.MapMatch(exception);

                Logger.Error(
                    BuildLogMessage(
                        nameof(GetGameStateAsync),
                        messageCode,
                        request == null ? 0 : request.AccountId,
                        request == null ? 0 : request.MatchId),
                    exception);

                return new GetMatchGameStateResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString(),
                    GameState = null
                };
            }
        }

        public async Task<GuessLetterResponse> GuessLetterAsync(
            GuessLetterRequest request)
        {
            try
            {
                GuessLetterResponse response = await matchGuessBusiness.GuessLetterAsync(request);

                NotifyMatchGuessChanged(
                    response?.GameState,
                    response != null && response.MatchFinished);

                return response;
            }
            catch (Exception exception)
            {
                MatchMessageCode messageCode =
                    ServiceExceptionMapper.MapMatch(exception);

                Logger.Error(
                    BuildLogMessage(
                        nameof(GuessLetterAsync),
                        messageCode,
                        request == null ? 0 : request.AccountId,
                        request == null ? 0 : request.MatchId),
                    exception);

                return new GuessLetterResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString(),
                    IsCorrect = false,
                    MatchFinished = false,
                    GameState = null
                };
            }
        }

        public async Task<GuessWordResponse> GuessWordAsync(
            GuessWordRequest request)
        {
            try
            {
                GuessWordResponse response = await matchGuessBusiness.GuessWordAsync(request);

                NotifyMatchGuessChanged(
                    response?.GameState,
                    response != null && response.MatchFinished);

                return response;
            }
            catch (Exception exception)
            {
                MatchMessageCode messageCode =
                    ServiceExceptionMapper.MapMatch(exception);

                Logger.Error(
                    BuildLogMessage(
                        nameof(GuessWordAsync),
                        messageCode,
                        request == null ? 0 : request.AccountId,
                        request == null ? 0 : request.MatchId),
                    exception);

                return new GuessWordResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString(),
                    IsCorrect = false,
                    MatchFinished = false,
                    GameState = null
                };
            }
        }

        public async Task<ResolveGuessTimeoutResponse> ResolveGuessTimeoutAsync(
            ResolveGuessTimeoutRequest request)
        {
            try
            {
                ResolveGuessTimeoutResponse response = await matchGuessBusiness.ResolveGuessTimeoutAsync(request);

                NotifyMatchGuessChanged(
                    response?.GameState,
                    response != null && response.MatchFinished);

                return response;
            }
            catch (Exception exception)
            {
                MatchMessageCode messageCode =
                    ServiceExceptionMapper.MapMatch(exception);

                Logger.Error(
                    BuildLogMessage(
                        nameof(ResolveGuessTimeoutAsync),
                        messageCode,
                        request == null ? 0 : request.AccountId,
                        request == null ? 0 : request.MatchId),
                    exception);

                return new ResolveGuessTimeoutResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString(),
                    MatchFinished = false,
                    GameState = null
                };
            }
        }

        private static void NotifyMatchGuessChanged(
            MatchGameStateDto gameState,
            bool matchFinished)
        {
            if (gameState == null || gameState.MatchId <= 0)
            {
                return;
            }

            MatchNotificationHub.NotifyLobbyUpdated(gameState.MatchId);

            if (matchFinished || gameState.IsFinished)
            {
                MatchNotificationHub.NotifyMatchStatusChanged(
                    gameState.MatchId,
                    gameState.MatchStatus);
            }
        }

        private static string BuildLogMessage(
            string operationName,
            MatchMessageCode messageCode,
            int accountId,
            int matchId)
        {
            return string.Format(
                "Error executing {0}. MessageCode: {1}. AccountId: {2}. MatchId: {3}",
                operationName,
                messageCode,
                accountId,
                matchId);
        }
    }
}