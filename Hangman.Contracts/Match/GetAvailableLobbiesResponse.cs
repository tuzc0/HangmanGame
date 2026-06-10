using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Hangman.Contracts.Match
{
    [DataContract]
    public class GetAvailableLobbiesResponse
    {
        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public string MessageCode { get; set; }

        [DataMember]
        public List<AvailableLobbyDto> Lobbies { get; set; }
    }
}
