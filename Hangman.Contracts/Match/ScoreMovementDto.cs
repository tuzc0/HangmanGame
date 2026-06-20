using System;
using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class ScoreMovementDto
    {
        [DataMember]
        public int ScoreMovementId { get; set; }

        [DataMember]
        public int Points { get; set; }

        [DataMember]
        public string MovementType { get; set; }

        [DataMember]
        public DateTime CreatedAt { get; set; }
    }
}
