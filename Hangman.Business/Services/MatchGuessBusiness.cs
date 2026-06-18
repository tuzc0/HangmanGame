using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using Hangman.Business.UserCases.MatchGuess;
using Hangman.Contracts.Match;
using System;
using System.Threading.Tasks;

namespace Hangman.Business.Services
{
    public class MatchGuessBusiness : IMatchGuessBusiness
    {
        private readonly GetMatchGameStateUseCase getMatchGameStateUseCase;
        private readonly GuessLetterUseCase guessLetterUseCase;
        private readonly GuessWordUseCase guessWordUseCase;
        private readonly ResolveGuessTimeoutUseCase resolveGuessTimeoutUseCase;

        public MatchGuessBusiness(IUnitOfWorkFactory unitOfWorkFactory)
        {
            if (unitOfWorkFactory == null)
            {
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
            }

            getMatchGameStateUseCase =
                new GetMatchGameStateUseCase(unitOfWorkFactory);

            guessLetterUseCase =
                new GuessLetterUseCase(unitOfWorkFactory);

            guessWordUseCase =
                new GuessWordUseCase(unitOfWorkFactory);

            resolveGuessTimeoutUseCase =
                new ResolveGuessTimeoutUseCase(unitOfWorkFactory);
        }

        public Task<GetMatchGameStateResponse> GetGameStateAsync(
            GetMatchGameStateRequest request)
        {
            return getMatchGameStateUseCase.ExecuteAsync(request);
        }

        public Task<GuessLetterResponse> GuessLetterAsync(
            GuessLetterRequest request)
        {
            return guessLetterUseCase.ExecuteAsync(request);
        }

        public Task<GuessWordResponse> GuessWordAsync(
            GuessWordRequest request)
        {
            return guessWordUseCase.ExecuteAsync(request);
        }

        public Task<ResolveGuessTimeoutResponse> ResolveGuessTimeoutAsync(
            ResolveGuessTimeoutRequest request)
        {
            return resolveGuessTimeoutUseCase.ExecuteAsync(request);
        }
    }
}
