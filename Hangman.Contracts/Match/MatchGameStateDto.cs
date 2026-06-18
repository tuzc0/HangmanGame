using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class MatchGameStateDto
    {
        [DataMember]
        public int MatchId { get; set; }

        [DataMember]
        public int HostId { get; set; }

        [DataMember]
        public string HostFullName { get; set; }

        [DataMember]
        public int? GuestId { get; set; }

        [DataMember]
        public string GuestFullName { get; set; }

        [DataMember]
        public string MatchStatus { get; set; }

        [DataMember]
        public int FailedAttempts { get; set; }

        [DataMember]
        public int MaxAttempts { get; set; }

        [DataMember]
        public DateTime? GuessTurnStartedAt { get; set; }

        [DataMember]
        public DateTime? GuessTurnEndsAt { get; set; }

        [DataMember]
        public int RemainingSeconds { get; set; }

        [DataMember]
        public bool IsFinished { get; set; }

        [DataMember]
        public int? WinnerId { get; set; }

        [DataMember]
        public string WinnerFullName { get; set; }

        [DataMember]
        public string WinnerEmail { get; set; }

        [DataMember]
        public List<LetterSlotDto> LetterSlots { get; set; }

        [DataMember]
        public string WordDescription { get; set; }

        [DataMember]
        public List<GuessHistoryDto> GuessHistory { get; set; }

        [DataMember]
        public HangmanFigureDto HangmanFigure { get; set; }
    }
}
