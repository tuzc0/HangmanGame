using Hangman.Contracts.Contracts;
using Hangman.Contracts.Match;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace HangmanGame.Services.Notifications
{
    public static class MatchNotificationHub
    {
        private static readonly ILog Log =
            LogManager.GetLogger(typeof(MatchNotificationHub));

        private static readonly object SyncRoot = new object();

        private static readonly Dictionary<int, List<MatchClientSubscription>> Subscriptions =
            new Dictionary<int, List<MatchClientSubscription>>();

        private static readonly List<MatchClientSubscription> AvailableLobbySubscriptions =
            new List<MatchClientSubscription>();

        public static void Subscribe(
            int matchId,
            int accountId,
            IMatchNotificationCallback callback)
        {
            if (matchId <= 0)
            {
                throw new ArgumentException("Match id must be greater than zero.", nameof(matchId));
            }

            if (accountId <= 0)
            {
                throw new ArgumentException("Account id must be greater than zero.", nameof(accountId));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            ICommunicationObject communicationObject = callback as ICommunicationObject;

            lock (SyncRoot)
            {
                RemoveSubscriptionInternal(matchId, accountId);

                if (!Subscriptions.ContainsKey(matchId))
                {
                    Subscriptions[matchId] = new List<MatchClientSubscription>();
                }

                MatchClientSubscription subscription = new MatchClientSubscription
                {
                    MatchId = matchId,
                    AccountId = accountId,
                    Callback = callback,
                    CommunicationObject = communicationObject,
                    SubscribedAt = DateTime.UtcNow
                };

                Subscriptions[matchId].Add(subscription);

                if (communicationObject != null)
                {
                    communicationObject.Closed += (sender, args) =>
                        Unsubscribe(matchId, accountId);

                    communicationObject.Faulted += (sender, args) =>
                        Unsubscribe(matchId, accountId);
                }
            }
        }

        public static void Unsubscribe(int matchId, int accountId)
        {
            lock (SyncRoot)
            {
                RemoveSubscriptionInternal(matchId, accountId);
            }
        }

        public static void RemoveMatch(int matchId)
        {
            lock (SyncRoot)
            {
                if (Subscriptions.ContainsKey(matchId))
                {
                    Subscriptions.Remove(matchId);
                }
            }
        }

        public static void NotifyLobbyUpdated(int matchId)
        {
            foreach (MatchClientSubscription subscription in GetSubscriptionsSnapshot(matchId))
            {
                try
                {
                    subscription.Callback.OnLobbyUpdated(matchId);
                }
                catch (Exception exception)
                {
                    Log.Error(
                        string.Format("Error notifying lobby update. MatchId: {0}. AccountId: {1}",
                        matchId,
                        subscription.AccountId),
                        exception);

                    Unsubscribe(matchId, subscription.AccountId);
                }
            }
        }

        public static void NotifyLobbyClosed(int matchId, string messageCode)
        {
            foreach (MatchClientSubscription subscription in GetSubscriptionsSnapshot(matchId))
            {
                try
                {
                    subscription.Callback.OnLobbyClosed(matchId, messageCode);
                }
                catch (Exception exception)
                {
                    Log.Error(
                        string.Format("Error notifying lobby closed. MatchId: {0}. AccountId: {1}",
                        matchId,
                        subscription.AccountId),
                        exception);
                }
            }

            RemoveMatch(matchId);
        }

        public static void NotifyMatchStatusChanged(int matchId, string matchStatus)
        {
            foreach (MatchClientSubscription subscription in GetSubscriptionsSnapshot(matchId))
            {
                try
                {
                    subscription.Callback.OnMatchStatusChanged(matchId, matchStatus);
                }
                catch (Exception exception)
                {
                    Log.Error(
                        string.Format("Error notifying match status changed. MatchId: {0}. AccountId: {1}",
                        matchId,
                        subscription.AccountId),
                        exception);

                    Unsubscribe(matchId, subscription.AccountId);
                }
            }
        }

        private static List<MatchClientSubscription> GetSubscriptionsSnapshot(int matchId)
        {
            lock (SyncRoot)
            {
                if (!Subscriptions.ContainsKey(matchId))
                {
                    return new List<MatchClientSubscription>();
                }

                return Subscriptions[matchId].ToList();
            }
        }

        private static void RemoveSubscriptionInternal(int matchId, int accountId)
        {
            if (!Subscriptions.ContainsKey(matchId))
            {
                return;
            }

            Subscriptions[matchId].RemoveAll(subscription =>
                subscription.AccountId == accountId);

            if (Subscriptions[matchId].Count == 0)
            {
                Subscriptions.Remove(matchId);
            }
        }

        public static void SubscribeAvailableLobbies(
            int accountId,
            IMatchNotificationCallback callback)
        {
            if (accountId <= 0)
            {
                throw new ArgumentException("Account id must be greater than zero.", nameof(accountId));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            ICommunicationObject communicationObject = callback as ICommunicationObject;

            lock (SyncRoot)
            {
                RemoveAvailableLobbySubscriptionInternal(accountId);

                MatchClientSubscription subscription = new MatchClientSubscription
                {
                    MatchId = 0,
                    AccountId = accountId,
                    Callback = callback,
                    CommunicationObject = communicationObject,
                    SubscribedAt = DateTime.UtcNow
                };

                AvailableLobbySubscriptions.Add(subscription);

                if (communicationObject != null)
                {
                    communicationObject.Closed += (sender, args) =>
                        UnsubscribeAvailableLobbies(accountId);

                    communicationObject.Faulted += (sender, args) =>
                        UnsubscribeAvailableLobbies(accountId);
                }
            }
        }

        public static void UnsubscribeAvailableLobbies(int accountId)
        {
            lock (SyncRoot)
            {
                RemoveAvailableLobbySubscriptionInternal(accountId);
            }
        }

        public static void NotifyAvailableLobbiesChanged()
        {
            foreach (MatchClientSubscription subscription in GetAvailableLobbySubscriptionsSnapshot())
            {
                try
                {
                    subscription.Callback.OnAvailableLobbiesChanged();
                }
                catch (Exception exception)
                {
                    Log.Error(
                        string.Format(
                        "Error notifying available lobbies changed. AccountId: {0}",
                        subscription.AccountId),
                        exception);

                    UnsubscribeAvailableLobbies(subscription.AccountId);
                }
            }
        }

        public static void NotifyChatMessageReceived(MatchChatMessageDto message)
        {
            if (message == null || message.MatchId <= 0)
            {
                return;
            }

            foreach (MatchClientSubscription subscription in GetSubscriptionsSnapshot(message.MatchId))
            {
                try
                {
                    subscription.Callback.OnMatchChatMessageReceived(message);
                }
                catch (Exception exception)
                {
                    Log.Error(
                        string.Format(
                            "Error notifying chat message. MatchId: {0}. AccountId: {1}",
                            message.MatchId,
                            subscription.AccountId),
                        exception);

                    Unsubscribe(message.MatchId, subscription.AccountId);
                }
            }
        }

        private static List<MatchClientSubscription> GetAvailableLobbySubscriptionsSnapshot()
        {
            lock (SyncRoot)
            {
                return AvailableLobbySubscriptions.ToList();
            }
        }

        private static void RemoveAvailableLobbySubscriptionInternal(int accountId)
        {
            AvailableLobbySubscriptions.RemoveAll(subscription =>
                subscription.AccountId == accountId);
        }
    }
}