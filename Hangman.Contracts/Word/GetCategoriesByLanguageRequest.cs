using System.Runtime.Serialization;

namespace Hangman.Contracts.Word
{
    [DataContract]
    public class GetCategoriesByLanguageRequest
    {
        [DataMember]
        public string LanguageCode { get; set; }
    }
}
