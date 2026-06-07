using System.Runtime.Serialization;

namespace Hangman.Contracts.Auth
{
    [DataContract]
    public class RequestPasswordResetRequest
    {
        [DataMember]
        public string Email { get; set; }
    }
}
