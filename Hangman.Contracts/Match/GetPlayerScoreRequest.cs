using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class GetPlayerScoreRequest
    {
        [DataMember]
        public int PlayerId { get; set; }
    }
}
