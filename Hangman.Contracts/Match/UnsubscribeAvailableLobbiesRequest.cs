using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class UnsubscribeAvailableLobbiesRequest
    {
        [DataMember]
        public int AccountId { get; set; }
    }
}
