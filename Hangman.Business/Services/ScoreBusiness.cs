using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using Hangman.Business.UserCases.Score;
using Hangman.Contracts.Match;
using System;
using System.Threading.Tasks;

namespace Hangman.Business.Services
{
    public class ScoreBusiness : IScoreBusiness
    {
        private readonly GetPlayerScoreUseCase getPlayerScoreUseCase;

        public ScoreBusiness(IUnitOfWorkFactory unitOfWorkFactory)
        {
            if (unitOfWorkFactory == null)
            {
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
            }

            getPlayerScoreUseCase = new GetPlayerScoreUseCase(unitOfWorkFactory);
        }

        public Task<GetPlayerScoreResponse> GetPlayerScoreAsync(GetPlayerScoreRequest request)
        {
            return getPlayerScoreUseCase.ExecuteAsync(request);
        }
    }
}
