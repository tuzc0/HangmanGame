using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class LetterSlotDto
    {
        [DataMember]
        public int Position { get; set; }

        [DataMember]
        public string Letter { get; set; }

        [DataMember]
        public bool IsRevealed { get; set; }
    }
}
