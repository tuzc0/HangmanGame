using System.Runtime.Serialization;

namespace Hangman.Contracts.Auth
{
    [DataContract]
    public class VerifyEmailRequest
    {
        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Code { get; set; }
    }
}
