using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using Hangman.Business.Services;
using Hangman.Contracts.Contracts;
using Hangman.Contracts.Match;
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
    public class MatchChatService : IMatchChatService
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(typeof(MatchChatService));

        private readonly IMatchChatBusiness matchChatBusiness;

        public MatchChatService()
            : this(new MatchChatBusiness(new UnitOfWorkFactory()))
        {
        }

        internal MatchChatService(IMatchChatBusiness matchChatBusiness)
        {
            this.matchChatBusiness = matchChatBusiness ??
                throw new ArgumentNullException(nameof(matchChatBusiness));
        }

        public async Task<SendMatchChatMessageResponse> SendMessageAsync(
            SendMatchChatMessageRequest request)
        {
            try
            {
                SendMatchChatMessageResponse response =
                    await matchChatBusiness.SendMessageAsync(request);

                if (response != null &&
                    response.Success &&
                    response.Message != null)
                {
                    MatchNotificationHub.NotifyChatMessageReceived(
                        response.Message);
                }

                return response;
            }
            catch (Exception exception)
            {
                Logger.Error(
                    BuildLogMessage(
                        nameof(SendMessageAsync),
                        request == null ? 0 : request.AccountId,
                        request == null ? 0 : request.MatchId),
                    exception);

                return new SendMatchChatMessageResponse
                {
                    Success = false,
                    MessageCode = "UnexpectedError",
                    Message = null
                };
            }
        }

        private static string BuildLogMessage(
            string operationName,
            int accountId,
            int matchId)
        {
            return string.Format(
                "Error executing {0}. AccountId: {1}. MatchId: {2}",
                operationName,
                accountId,
                matchId);
        }
    }
}