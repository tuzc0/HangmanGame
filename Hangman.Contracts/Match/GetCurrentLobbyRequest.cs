using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class GetCurrentLobbyRequest
    {
        [DataMember]
        public int AccountId { get; set; }
    }
}
