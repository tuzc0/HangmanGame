using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class CategoryVotingStateDto
    {
        [DataMember]
        public int MatchId { get; set; }

        [DataMember]
        public string MatchStatus { get; set; }

        [DataMember]
        public int? SelectedCategoryId { get; set; }

        [DataMember]
        public string SelectedCategoryName { get; set; }

        [DataMember]
        public DateTime? CategoryVotingStartedAt { get; set; }

        [DataMember]
        public DateTime? CategoryVotingEndsAt { get; set; }

        [DataMember]
        public DateTime? WordSelectionStartedAt { get; set; }

        [DataMember]
        public DateTime? WordSelectionEndsAt { get; set; }

        [DataMember]
        public int RemainingVotingSeconds { get; set; }

        [DataMember]
        public bool CanVote { get; set; }

        [DataMember]
        public bool IsVotingResolved { get; set; }

        [DataMember]
        public bool CanCurrentPlayerSelectWord { get; set; }

        [DataMember]
        public List<CategoryVoteDto> Votes { get; set; }
    }
}
