using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class JoinLobbyRequest
    {
        [DataMember]
        public int MatchId { get; set; }

        [DataMember]
        public int GuestAccountId { get; set; }

        [DataMember]
        public string GuestLanguageCode { get; set; }
    }
}
