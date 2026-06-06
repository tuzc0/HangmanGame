using System.Runtime.Serialization;

namespace Hangman.Contracts.Auth
{
    [DataContract]
    public class LoginResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string MessageCode { get; set; }

        [DataMember]
        public AuthenticatedPlayerDto Player { get; set; }
    }
}
