using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class GetAvailableLobbiesRequest
    {
        [DataMember]
        public int AccountId { get; set; }
    }
}
