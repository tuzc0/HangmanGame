using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using Hangman.Contracts.Match;
using Hangman.DataAccess.Interfaces;
using Hangman.DataAccess.Transporters;
using System;
using System.Threading.Tasks;

namespace Hangman.Business.Services
{
    public sealed class MatchChatBusiness : IMatchChatBusiness
    {
        private const int MaximumMessageLength = 250;

        private const string ChatMessageSentCode = "ChatMessageSent";
        private const string InvalidMatchIdCode = "InvalidMatchId";
        private const string InvalidChatMessageCode = "InvalidChatMessage";
        private const string SessionInvalidCode = "SessionInvalid";
        private const string PlayerNotInMatchCode = "PlayerNotInMatch";
        private const string UnexpectedErrorCode = "UnexpectedError";

        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public MatchChatBusiness(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<SendMatchChatMessageResponse> SendMessageAsync(
            SendMatchChatMessageRequest request)
        {
            if (request == null || request.AccountId <= 0)
            {
                return Failure(SessionInvalidCode);
            }

            if (request.MatchId <= 0)
            {
                return Failure(InvalidMatchIdCode);
            }

            string normalizedMessage = NormalizeMessage(request.Message);

            if (string.IsNullOrWhiteSpace(normalizedMessage))
            {
                return Failure(InvalidChatMessageCode);
            }

            using (IUnitOfWork unitOfWork = unitOfWorkFactory.Create())
            {
                AccountTransporter account =
                    await unitOfWork.Accounts.GetByIdAsync(request.AccountId);

                if (account == null || account.PlayerId <= 0)
                {
                    return Failure(SessionInvalidCode);
                }

                bool belongsToMatch =
                    await unitOfWork.Matches.IsPlayerInMatchAsync(
                        request.MatchId,
                        account.PlayerId);

                if (!belongsToMatch)
                {
                    return Failure(PlayerNotInMatchCode);
                }

                PlayerTransporter player =
                    await unitOfWork.Players.GetByIdAsync(account.PlayerId);

                if (player == null)
                {
                    return Failure(SessionInvalidCode);
                }

                MatchChatMessageDto message = new MatchChatMessageDto
                {
                    MatchId = request.MatchId,
                    SenderAccountId = request.AccountId,
                    SenderPlayerId = account.PlayerId,
                    SenderFullName = player.FullName,
                    Message = normalizedMessage,
                    SentAt = DateTime.UtcNow
                };

                return new SendMatchChatMessageResponse
                {
                    Success = true,
                    MessageCode = ChatMessageSentCode,
                    Message = message
                };
            }
        }

        private static string NormalizeMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return string.Empty;
            }

            string normalized = message.Trim();

            if (normalized.Length > MaximumMessageLength)
            {
                return normalized.Substring(0, MaximumMessageLength);
            }

            return normalized;
        }

        private static SendMatchChatMessageResponse Failure(string messageCode)
        {
            return new SendMatchChatMessageResponse
            {
                Success = false,
                MessageCode = string.IsNullOrWhiteSpace(messageCode)
                    ? UnexpectedErrorCode
                    : messageCode,
                Message = null
            };
        }
    }
}
