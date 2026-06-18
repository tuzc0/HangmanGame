using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class GuessWordRequest
    {
        [DataMember]
        public int MatchId { get; set; }

        [DataMember]
        public int AccountId { get; set; }

        [DataMember]
        public string Word { get; set; }
    }
}
