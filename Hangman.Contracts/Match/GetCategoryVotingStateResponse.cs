using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class GetCategoryVotingStateResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string MessageCode { get; set; }

        [DataMember]
        public CategoryVotingStateDto VotingState { get; set; }
    }
}
