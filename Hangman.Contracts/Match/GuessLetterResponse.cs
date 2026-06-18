using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class GuessLetterResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string MessageCode { get; set; }

        [DataMember]
        public bool IsCorrect { get; set; }

        [DataMember]
        public bool MatchFinished { get; set; }

        [DataMember]
        public MatchGameStateDto GameState { get; set; }
    }
}
