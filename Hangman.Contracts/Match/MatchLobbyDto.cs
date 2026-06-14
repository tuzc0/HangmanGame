using System;
using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class MatchLobbyDto
    {
        [DataMember]
        public int MatchId { get; set; }

        [DataMember]
        public int HostId { get; set; }

        [DataMember]
        public string HostFullName { get; set; }

        [DataMember]
        public string HostLanguageCode { get; set; }

        [DataMember]
        public int? GuestId { get; set; }

        [DataMember]
        public string GuestFullName { get; set; }

        [DataMember]
        public string GuestLanguageCode { get; set; }

        [DataMember]
        public int? SelectedCategoryId { get; set; }

        [DataMember]
        public string SelectedCategoryName { get; set; }

        [DataMember]
        public int? SelectedWordId { get; set; }

        [DataMember]
        public DateTime? WordSelectionStartedAt { get; set; }

        [DataMember]
        public DateTime? WordSelectionEndsAt { get; set; }

        [DataMember]
        public DateTime? StartedAt { get; set; }

        [DataMember]
        public DateTime? FinishedAt { get; set; }

        [DataMember]
        public int? WinnerId { get; set; }

        [DataMember]
        public int? PenalizedUserId { get; set; }

        [DataMember]
        public string MatchStatus { get; set; }

        [DataMember]
        public DateTime CreatedAt { get; set; }

        [DataMember]
        public DateTime? JoinedAt { get; set; }

        [DataMember]
        public DateTime? CategoryVotingStartedAt { get; set; }

        [DataMember]
        public DateTime? CategoryVotingEndsAt { get; set; }
    }
}
