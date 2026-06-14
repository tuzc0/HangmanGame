using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using Hangman.Business.Messages;
using Hangman.Business.Services;
using Hangman.Contracts.Contracts;
using Hangman.Contracts.Match;
using HangmanGame.Services.ExceptionHandling;
using HangmanGame.Services.Notifications;
using log4net;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace HangmanGame.Services.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class MatchGameplayService : IMatchGameplayService
    {
        private static readonly ILog Log =
            LogManager.GetLogger(typeof(MatchGameplayService));

        private readonly IMatchGameplayBusiness matchGameplayBusiness;

        public MatchGameplayService()
            : this(new MatchGameplayBusiness(new UnitOfWorkFactory()))
        {
        }

        internal MatchGameplayService(IMatchGameplayBusiness matchGameplayBusiness)
        {
            this.matchGameplayBusiness = matchGameplayBusiness ??
                throw new ArgumentNullException(nameof(matchGameplayBusiness));
        }

        public async Task<VoteCategoryResponse> VoteCategoryAsync(
            VoteCategoryRequest request)
        {
            try
            {
                VoteCategoryResponse response =
                    await matchGameplayBusiness.VoteCategoryAsync(request);

                if (response != null &&
                    response.VotingState != null &&
                    request != null &&
                    response.Success)
                {
                    MatchNotificationHub.NotifyLobbyUpdated(request.MatchId);

                    NotifyStatusIfResolved(response.VotingState);
                }

                return response;
            }
            catch (Exception exception)
            {
                MatchMessageCode messageCode =
                    ServiceExceptionMapper.MapMatch(exception);

                Log.Error(
                    string.Format(
                        "Error executing VoteCategoryAsync. MessageCode: {0}. MatchId: {1}. AccountId: {2}. CategoryId: {3}",
                        messageCode,
                        request != null ? request.MatchId : 0,
                        request != null ? request.AccountId : 0,
                        request != null ? request.CategoryId : 0),
                    exception);

                return new VoteCategoryResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString(),
                    VotingState = null
                };
            }
        }

        public async Task<GetCategoryVotingStateResponse> GetCategoryVotingStateAsync(
            GetCategoryVotingStateRequest request)
        {
            try
            {
                GetCategoryVotingStateResponse response =
                    await matchGameplayBusiness.GetCategoryVotingStateAsync(request);

                if (response != null &&
                    response.Success &&
                    response.VotingState != null)
                {
                    NotifyStatusIfResolved(response.VotingState);
                }

                return response;
            }
            catch (Exception exception)
            {
                MatchMessageCode messageCode =
                    ServiceExceptionMapper.MapMatch(exception);

                Log.Error(
                    string.Format(
                        "Error executing GetCategoryVotingStateAsync. MessageCode: {0}. MatchId: {1}. AccountId: {2}",
                        messageCode,
                        request != null ? request.MatchId : 0,
                        request != null ? request.AccountId : 0),
                    exception);

                return new GetCategoryVotingStateResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString(),
                    VotingState = null
                };
            }
        }

        public async Task<ResolveCategoryVotingResponse> ResolveCategoryVotingAsync(
            ResolveCategoryVotingRequest request)
        {
            try
            {
                ResolveCategoryVotingResponse response =
                    await matchGameplayBusiness.ResolveCategoryVotingAsync(request);

                if (response != null &&
                    response.Success &&
                    response.Lobby != null)
                {
                    MatchNotificationHub.NotifyLobbyUpdated(response.Lobby.MatchId);

                    MatchNotificationHub.NotifyMatchStatusChanged(
                        response.Lobby.MatchId,
                        response.Lobby.MatchStatus);
                }

                return response;
            }
            catch (Exception exception)
            {
                MatchMessageCode messageCode =
                    ServiceExceptionMapper.MapMatch(exception);

                Log.Error(
                    string.Format(
                        "Error executing ResolveCategoryVotingAsync. MessageCode: {0}. MatchId: {1}. AccountId: {2}",
                        messageCode,
                        request != null ? request.MatchId : 0,
                        request != null ? request.AccountId : 0),
                    exception);

                return new ResolveCategoryVotingResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString(),
                    Lobby = null,
                    VotingState = null
                };
            }
        }

        public async Task<GetSelectableWordsResponse> GetSelectableWordsAsync(
            GetSelectableWordsRequest request)
        {
            try
            {
                return await matchGameplayBusiness.GetSelectableWordsAsync(request);
            }
            catch (Exception exception)
            {
                MatchMessageCode messageCode =
                    ServiceExceptionMapper.MapMatch(exception);

                Log.Error(
                    string.Format(
                        "Error executing GetSelectableWordsAsync. MessageCode: {0}. MatchId: {1}. AccountId: {2}",
                        messageCode,
                        request != null ? request.MatchId : 0,
                        request != null ? request.AccountId : 0),
                    exception);

                return new GetSelectableWordsResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString(),
                    Words = new List<SelectableWordDto>()
                };
            }
        }

        public async Task<SelectWordResponse> SelectWordAsync(
            SelectWordRequest request)
        {
            try
            {
                SelectWordResponse response =
                    await matchGameplayBusiness.SelectWordAsync(request);

                if (response != null &&
                    response.Success &&
                    response.Lobby != null)
                {
                    MatchNotificationHub.NotifyLobbyUpdated(response.Lobby.MatchId);

                    MatchNotificationHub.NotifyMatchStatusChanged(
                        response.Lobby.MatchId,
                        response.Lobby.MatchStatus);
                }

                return response;
            }
            catch (Exception exception)
            {
                MatchMessageCode messageCode =
                    ServiceExceptionMapper.MapMatch(exception);

                Log.Error(
                    string.Format(
                        "Error executing SelectWordAsync. MessageCode: {0}. MatchId: {1}. AccountId: {2}. WordId: {3}",
                        messageCode,
                        request != null ? request.MatchId : 0,
                        request != null ? request.AccountId : 0,
                        request != null ? request.WordId : 0),
                    exception);

                return new SelectWordResponse
                {
                    Success = false,
                    MessageCode = messageCode.ToString(),
                    Lobby = null
                };
            }
        }

        private static void NotifyStatusIfResolved(CategoryVotingStateDto votingState)
        {
            if (votingState == null || !votingState.IsVotingResolved)
            {
                return;
            }

            MatchNotificationHub.NotifyLobbyUpdated(votingState.MatchId);

            MatchNotificationHub.NotifyMatchStatusChanged(
                votingState.MatchId,
                votingState.MatchStatus);
        }
    }
}