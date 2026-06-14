using Hangman.Contracts.Match;
using System;
using System.Collections.Generic;

namespace Hangman.Business.Mappers
{
    internal static class MatchResponseFactory
    {
        public static CreateLobbyResponse BuildCreateLobbyResponse(
            bool success,
            Enum messageCode,
            MatchLobbyDto lobby)
        {
            return new CreateLobbyResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Lobby = lobby
            };
        }

        public static GetAvailableLobbiesResponse BuildGetAvailableLobbiesResponse(
            bool success,
            Enum messageCode,
            List<AvailableLobbyDto> lobbies)
        {
            return new GetAvailableLobbiesResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Lobbies = lobbies
            };
        }

        public static JoinLobbyResponse BuildJoinLobbyResponse(
            bool success,
            Enum messageCode,
            MatchLobbyDto lobby)
        {
            return new JoinLobbyResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Lobby = lobby
            };
        }

        public static GetCurrentLobbyResponse BuildGetCurrentLobbyResponse(
            bool success,
            Enum messageCode,
            MatchLobbyDto lobby)
        {
            return new GetCurrentLobbyResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Lobby = lobby
            };
        }

        public static LeaveLobbyResponse BuildLeaveLobbyResponse(
            bool success,
            Enum messageCode)
        {
            return new LeaveLobbyResponse
            {
                Success = success,
                MessageCode = messageCode.ToString()
            };
        }
    }
}
