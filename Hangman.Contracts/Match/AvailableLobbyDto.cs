using System;
using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class AvailableLobbyDto
    {
        [DataMember]
        public int MatchId { get; set; }

        [DataMember]
        public int HostId { get; set; }

        [DataMember]
        public string HostFullName { get; set; }

        [DataMember]
        public string HostLanguageCode { get; set; }

        [DataMember]
        public string MatchStatus { get; set; }

        [DataMember]
        public DateTime CreatedAt { get; set; }
    }
}
