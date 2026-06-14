using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class GetSelectableWordsResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string MessageCode { get; set; }

        [DataMember]
        public List<SelectableWordDto> Words { get; set; }
    }
}
