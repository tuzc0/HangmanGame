using System.Runtime.Serialization;

namespace Hangman.Contracts.Profile
{
    [DataContract]
    public class DeleteProfileRequest
    {
        [DataMember]
        public int AccountId { get; set; }
    }
}
