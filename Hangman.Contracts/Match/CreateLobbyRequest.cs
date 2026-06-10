using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class CreateLobbyRequest
    {
        [DataMember]
        public int HostAccountId { get; set; }

        [DataMember]
        public string HostLanguageCode { get; set; }
    }
}
