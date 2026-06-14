using Hangman.DataAccess.Transporters;
using System;

namespace Hangman.Business.Results
{
    internal sealed class PlayerAvailabilityResult
    {
        public bool IsAvailable { get; private set; }

        public Enum MessageCode { get; private set; }

        public AccountTransporter Account { get; private set; }

        public PlayerTransporter Player { get; private set; }

        public static PlayerAvailabilityResult Success(
            AccountTransporter account,
            PlayerTransporter player)
        {
            return new PlayerAvailabilityResult
            {
                IsAvailable = true,
                MessageCode = null,
                Account = account,
                Player = player
            };
        }

        public static PlayerAvailabilityResult Fail(Enum messageCode)
        {
            if (messageCode == null)
            {
                throw new ArgumentNullException(nameof(messageCode));
            }

            return new PlayerAvailabilityResult
            {
                IsAvailable = false,
                MessageCode = messageCode,
                Account = null,
                Player = null
            };
        }
    }
}
