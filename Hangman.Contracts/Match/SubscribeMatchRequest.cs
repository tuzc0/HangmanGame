using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class SubscribeMatchRequest
    {
        [DataMember]
        public int MatchId { get; set; }

        [DataMember]
        public int AccountId { get; set; }
    }
}
