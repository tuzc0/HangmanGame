using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class SelectWordResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string MessageCode { get; set; }

        [DataMember]
        public MatchLobbyDto Lobby { get; set; }
    }
}
