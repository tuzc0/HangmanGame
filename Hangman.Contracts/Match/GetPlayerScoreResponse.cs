using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class GetPlayerScoreResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string MessageCode { get; set; }

        [DataMember]
        public int TotalScore { get; set; }

        [DataMember]
        public int GuesserWinTotal { get; set; }

        [DataMember]
        public int GuesserWinCount { get; set; }

        [DataMember]
        public int HostWinTotal { get; set; }

        [DataMember]
        public int HostWinCount { get; set; }

        [DataMember]
        public int PenaltyTotal { get; set; }

        [DataMember]
        public int PenaltyCount { get; set; }

        [DataMember]
        public List<ScoreMovementDto> Movements { get; set; }
    }
}
