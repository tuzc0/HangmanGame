using System.Runtime.Serialization;

namespace Hangman.Contracts.Auth
{
    [DataContract]
    public class RegisterResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string MessageCode { get; set; }

        [DataMember]
        public int AccountId { get; set; }

        [DataMember]
        public int PlayerId { get; set; }

        [DataMember]
        public bool RequiresEmailVerification { get; set; }

        [DataMember]
        public bool VerificationEmailSent { get; set; }
    }
}
