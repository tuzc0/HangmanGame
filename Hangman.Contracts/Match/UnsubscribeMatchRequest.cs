using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class UnsubscribeMatchRequest
    {
        [DataMember]
        public int MatchId { get; set; }

        [DataMember]
        public int AccountId { get; set; }
    }
}
