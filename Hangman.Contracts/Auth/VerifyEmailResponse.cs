using System.Runtime.Serialization;

namespace Hangman.Contracts.Auth
{
    [DataContract]
    public class VerifyEmailResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string MessageCode { get; set; }

        [DataMember]
        public int AccountId { get; set; }

        [DataMember]
        public bool IsEmailVerified { get; set; }
    }
}
