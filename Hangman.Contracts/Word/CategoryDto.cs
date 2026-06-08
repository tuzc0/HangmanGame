using System.Runtime.Serialization;

namespace Hangman.Contracts.Word
{
    [DataContract]
    public class CategoryDto
    {
        [DataMember]
        public int CategoryId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string LanguageCode { get; set; }
    }
}
