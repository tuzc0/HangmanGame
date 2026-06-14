using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class SelectableWordDto
    {
        [DataMember]
        public int WordId { get; set; }

        [DataMember]
        public int CategoryId { get; set; }

        [DataMember]
        public string CategoryName { get; set; }

        [DataMember]
        public string WordText { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string LanguageCode { get; set; }
    }
}
