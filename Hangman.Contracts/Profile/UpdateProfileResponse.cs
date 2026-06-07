using System.Runtime.Serialization;

namespace Hangman.Contracts.Profile
{
    [DataContract]
    public class UpdateProfileResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string MessageCode { get; set; }

        [DataMember]
        public ProfileDto Profile { get; set; }
    }
}
