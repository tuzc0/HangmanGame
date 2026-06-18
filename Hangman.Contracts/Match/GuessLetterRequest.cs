using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class GuessLetterRequest
    {
        [DataMember]
        public int MatchId { get; set; }

        [DataMember]
        public int AccountId { get; set; }

        [DataMember]
        public string Letter { get; set; }
    }
}
