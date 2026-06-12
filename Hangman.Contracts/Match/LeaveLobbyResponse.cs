using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class LeaveLobbyResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string MessageCode { get; set; }
    }
}
