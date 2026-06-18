using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class HangmanFigureDto
    {
        [DataMember]
        public int FailedAttempts { get; set; }

        [DataMember]
        public int MaxAttempts { get; set; }

        [DataMember]
        public bool ShowHead { get; set; }

        [DataMember]
        public bool ShowTorso { get; set; }

        [DataMember]
        public bool ShowLeftArm { get; set; }

        [DataMember]
        public bool ShowRightArm { get; set; }

        [DataMember]
        public bool ShowLeftLeg { get; set; }

        [DataMember]
        public bool ShowRightLeg { get; set; }
    }
}
