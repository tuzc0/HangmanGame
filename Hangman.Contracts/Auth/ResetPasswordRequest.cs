using System.Runtime.Serialization;

namespace Hangman.Contracts.Auth
{
    [DataContract]
    public class ResetPasswordRequest
    {
        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public string NewPassword { get; set; }
    }
}
