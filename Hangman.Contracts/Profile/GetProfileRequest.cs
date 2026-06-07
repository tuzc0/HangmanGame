using System.Runtime.Serialization;

namespace Hangman.Contracts.Profile
{
    [DataContract]
    public class GetProfileRequest
    {
        [DataMember]
        public int AccountId { get; set; }
    }
}
