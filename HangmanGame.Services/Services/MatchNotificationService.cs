using Hangman.Business.Messages;
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
        InstanceContextMode = InstanceContextMode.PerSession,
        ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class MatchNotificationService : IMatchNotificationService
    {
        private static readonly ILog Log =
            LogManager.GetLogger(typeof(MatchNotificationService));

        public Task<SubscribeMatchResponse> SubscribeAsync(
            SubscribeMatchRequest request)
        {
            try
            {
                if (request == null || request.MatchId <= 0 || request.AccountId <= 0)
                {
                    return Task.FromResult(new SubscribeMatchResponse
                    {
                        Success = false,
                        MessageCode = MatchMessageCode.LobbySubscriptionFailed.ToString()
                    });
                }

                IMatchNotificationCallback callback =
                    OperationContext.Current.GetCallbackChannel<IMatchNotificationCallback>();

                MatchNotificationHub.Subscribe(
                    request.MatchId,
                    request.AccountId,
                    callback);

                return Task.FromResult(new SubscribeMatchResponse
                {
                    Success = true,
                    MessageCode = MatchMessageCode.LobbySubscribed.ToString()
                });
            }
            catch (Exception exception)
            {
                Log.Error("Error subscribing match notification callback.", exception);

                return Task.FromResult(new SubscribeMatchResponse
                {
                    Success = false,
                    MessageCode = MatchMessageCode.LobbySubscriptionFailed.ToString()
                });
            }
        }

        public Task<UnsubscribeMatchResponse> UnsubscribeAsync(
            UnsubscribeMatchRequest request)
        {
            try
            {
                if (request == null || request.MatchId <= 0 || request.AccountId <= 0)
                {
                    return Task.FromResult(new UnsubscribeMatchResponse
                    {
                        Success = false,
                        MessageCode = MatchMessageCode.LobbyUnsubscriptionFailed.ToString()
                    });
                }

                MatchNotificationHub.Unsubscribe(
                    request.MatchId,
                    request.AccountId);

                return Task.FromResult(new UnsubscribeMatchResponse
                {
                    Success = true,
                    MessageCode = MatchMessageCode.LobbyUnsubscribed.ToString()
                });
            }
            catch (Exception exception)
            {
                Log.Error("Error unsubscribing match notification callback.", exception);

                return Task.FromResult(new UnsubscribeMatchResponse
                {
                    Success = false,
                    MessageCode = MatchMessageCode.LobbyUnsubscriptionFailed.ToString()
                });
            }
        }

        public Task<SubscribeAvailableLobbiesResponse> SubscribeAvailableLobbiesAsync(
            SubscribeAvailableLobbiesRequest request)
        {
            try
            {
                if (request == null || request.AccountId <= 0)
                {
                    return Task.FromResult(new SubscribeAvailableLobbiesResponse
                    {
                        Success = false,
                        MessageCode = MatchMessageCode.AvailableLobbySubscriptionFailed.ToString()
                    });
                }

                IMatchNotificationCallback callback =
                    OperationContext.Current.GetCallbackChannel<IMatchNotificationCallback>();

                MatchNotificationHub.SubscribeAvailableLobbies(
                    request.AccountId,
                    callback);

                return Task.FromResult(new SubscribeAvailableLobbiesResponse
                {
                    Success = true,
                    MessageCode = MatchMessageCode.AvailableLobbySubscribed.ToString()
                });
            }
            catch (Exception exception)
            {
                Log.Error("Error subscribing available lobbies notification callback.", exception);

                return Task.FromResult(new SubscribeAvailableLobbiesResponse
                {
                    Success = false,
                    MessageCode = MatchMessageCode.AvailableLobbySubscriptionFailed.ToString()
                });
            }
        }

        public Task<UnsubscribeAvailableLobbiesResponse> UnsubscribeAvailableLobbiesAsync(
            UnsubscribeAvailableLobbiesRequest request)
        {
            try
            {
                if (request == null || request.AccountId <= 0)
                {
                    return Task.FromResult(new UnsubscribeAvailableLobbiesResponse
                    {
                        Success = false,
                        MessageCode = MatchMessageCode.AvailableLobbyUnsubscriptionFailed.ToString()
                    });
                }

                MatchNotificationHub.UnsubscribeAvailableLobbies(request.AccountId);

                return Task.FromResult(new UnsubscribeAvailableLobbiesResponse
                {
                    Success = true,
                    MessageCode = MatchMessageCode.AvailableLobbyUnsubscribed.ToString()
                });
            }
            catch (Exception exception)
            {
                Log.Error("Error unsubscribing available lobbies notification callback.", exception);

                return Task.FromResult(new UnsubscribeAvailableLobbiesResponse
                {
                    Success = false,
                    MessageCode = MatchMessageCode.AvailableLobbyUnsubscriptionFailed.ToString()
                });
            }
        }
    }
}