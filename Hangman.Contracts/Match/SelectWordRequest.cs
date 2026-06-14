using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class SelectWordRequest
    {
        [DataMember]
        public int MatchId { get; set; }

        [DataMember]
        public int AccountId { get; set; }

        [DataMember]
        public int WordId { get; set; }
    }
}
