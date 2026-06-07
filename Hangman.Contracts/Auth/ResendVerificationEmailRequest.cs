using System.Runtime.Serialization;

namespace Hangman.Contracts.Auth
{
    [DataContract]
    public class ResendVerificationEmailRequest
    {
        [DataMember]
        public string Email { get; set; }
    }
}
