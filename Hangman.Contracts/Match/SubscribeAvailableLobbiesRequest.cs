using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class SubscribeAvailableLobbiesRequest
    {
        [DataMember]
        public int AccountId { get; set; }
    }
}
