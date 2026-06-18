using System;
using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class MatchChatMessageDto
    {
        [DataMember]
        public int MatchId { get; set; }

        [DataMember]
        public int SenderAccountId { get; set; }

        [DataMember]
        public int SenderPlayerId { get; set; }

        [DataMember]
        public string SenderFullName { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public DateTime SentAt { get; set; }
    }
}
