using System;
using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class GuessHistoryDto
    {
        [DataMember]
        public string GuessType { get; set; }

        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public bool IsCorrect { get; set; }

        [DataMember]
        public DateTime CreatedAt { get; set; }
    }
}
