using System;
using System.Runtime.Serialization;

namespace Hangman.Contracts.Auth
{
    [DataContract]
    public class RegisterRequest
    {
        [DataMember]
        public string FullName { get; set; }

        [DataMember]
        public DateTime DateOfBirth { get; set; }

        [DataMember]
        public string Phone { get; set; }

        [DataMember]
        public string PreferredLanguageCode { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Password { get; set; }
    }
}
