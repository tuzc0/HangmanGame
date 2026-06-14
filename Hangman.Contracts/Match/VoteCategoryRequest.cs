using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class VoteCategoryRequest
    {
        [DataMember]
        public int MatchId { get; set; }

        [DataMember]
        public int AccountId { get; set; }

        [DataMember]
        public int CategoryId { get; set; }
    }
}
