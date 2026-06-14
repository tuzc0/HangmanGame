using System;
using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class CategoryVoteDto
    {
        [DataMember]
        public int PlayerId { get; set; }

        [DataMember]
        public int CategoryId { get; set; }

        [DataMember]
        public string CategoryName { get; set; }

        [DataMember]
        public string LanguageCode { get; set; }

        [DataMember]
        public DateTime CreatedAt { get; set; }

        [DataMember]
        public DateTime? UpdatedAt { get; set; }
    }
}
