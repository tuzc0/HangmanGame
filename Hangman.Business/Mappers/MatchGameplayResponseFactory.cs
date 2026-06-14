using Hangman.Contracts.Match;
using System;
using System.Collections.Generic;

namespace Hangman.Business.Mappers
{
    internal static class MatchGameplayResponseFactory
    {
        public static VoteCategoryResponse BuildVoteCategoryResponse(
            bool success,
            Enum messageCode,
            CategoryVotingStateDto votingState)
        {
            return new VoteCategoryResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                VotingState = votingState
            };
        }

        public static GetCategoryVotingStateResponse BuildGetCategoryVotingStateResponse(
            bool success,
            Enum messageCode,
            CategoryVotingStateDto votingState)
        {
            return new GetCategoryVotingStateResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                VotingState = votingState
            };
        }

        public static ResolveCategoryVotingResponse BuildResolveCategoryVotingResponse(
            bool success,
            Enum messageCode,
            MatchLobbyDto lobby,
            CategoryVotingStateDto votingState)
        {
            return new ResolveCategoryVotingResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Lobby = lobby,
                VotingState = votingState
            };
        }

        public static GetSelectableWordsResponse BuildGetSelectableWordsResponse(
            bool success,
            Enum messageCode,
            List<SelectableWordDto> words)
        {
            return new GetSelectableWordsResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Words = words
            };
        }

        public static SelectWordResponse BuildSelectWordResponse(
            bool success,
            Enum messageCode,
            MatchLobbyDto lobby)
        {
            return new SelectWordResponse
            {
                Success = success,
                MessageCode = messageCode.ToString(),
                Lobby = lobby
            };
        }
    }
}
