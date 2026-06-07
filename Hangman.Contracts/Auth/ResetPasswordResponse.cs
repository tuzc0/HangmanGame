using System.Runtime.Serialization;

namespace Hangman.Contracts.Auth
{
    [DataContract]
    public class ResetPasswordResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string MessageCode { get; set; }
    }
}
