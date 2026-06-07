using System;
using System.Runtime.Serialization;

namespace Hangman.Contracts.Profile
{
    [DataContract]
    public class UpdateProfileRequest
    {
        [DataMember]
        public int AccountId { get; set; }

        [DataMember]
        public string FullName { get; set; }

        [DataMember]
        public DateTime DateOfBirth { get; set; }

        [DataMember]
        public string Phone { get; set; }

        [DataMember]
        public string PreferredLanguageCode { get; set; }
    }
}
