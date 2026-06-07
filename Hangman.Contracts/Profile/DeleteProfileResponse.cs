using System.Runtime.Serialization;

namespace Hangman.Contracts.Profile
{
    [DataContract]
    public class DeleteProfileResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string MessageCode { get; set; }
    }
}
