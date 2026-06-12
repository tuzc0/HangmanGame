using Hangman.Contracts.Contracts;
using System;
using System.ServiceModel;

namespace HangmanGame.Services.Notifications
{
    internal sealed class MatchClientSubscription
    {
        public int MatchId { get; set; }

        public int AccountId { get; set; }

        public IMatchNotificationCallback Callback { get; set; }

        public ICommunicationObject CommunicationObject { get; set; }

        public DateTime SubscribedAt { get; set; }
    }
}