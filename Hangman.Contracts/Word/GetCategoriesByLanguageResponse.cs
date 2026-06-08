using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Hangman.Contracts.Word
{
    [DataContract]
    public class GetCategoriesByLanguageResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string MessageCode { get; set; }

        [DataMember]
        public List<CategoryDto> Categories { get; set; }
    }
}
