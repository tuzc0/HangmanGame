using Hangman.Business.Constants;
using Hangman.Business.Factories;
using Hangman.Business.Mappers;
using Hangman.Business.Messages;
using Hangman.Business.Results;
using Hangman.Business.Validators;
using Hangman.Contracts.Match;
using Hangman.DataAccess.Transporters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hangman.Business.UserCases.Score
{
    internal class GetPlayerScoreUseCase
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

        public GetPlayerScoreUseCase(IUnitOfWorkFactory unitOfWorkFactory)
        {
            this.unitOfWorkFactory = unitOfWorkFactory ??
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public async Task<GetPlayerScoreResponse> ExecuteAsync(GetPlayerScoreRequest request)
        {
            ValidationResult validationResult = ScoreValidator.ValidateGetPlayerScore(request);

            if (!validationResult.IsValid)
            {
                return ScoreResponseFactory.BuildFailResponse(validationResult.MessageCode);
            }

            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                bool playerExists = await unitOfWork.Players.ExistsAsync(request.PlayerId);

                if (!playerExists)
                {
                    return ScoreResponseFactory.BuildFailResponse(ScoreMessageCode.PlayerNotFound);
                }

                List<ScoreMovementTransporter> movements =
                    await unitOfWork.ScoreMovements.GetByPlayerIdAsync(request.PlayerId);

                int totalScore = movements.Sum(m => m.Points);

                int guesserWinTotal = movements
                    .Where(m => m.MovementType == ScoreMovementTypeConstants.GuesserWin)
                    .Sum(m => m.Points);

                int guesserWinCount = movements
                    .Count(m => m.MovementType == ScoreMovementTypeConstants.GuesserWin);

                int hostWinTotal = movements
                    .Where(m => m.MovementType == ScoreMovementTypeConstants.HostWin)
                    .Sum(m => m.Points);

                int hostWinCount = movements
                    .Count(m => m.MovementType == ScoreMovementTypeConstants.HostWin);

                int penaltyTotal = movements
                    .Where(m => m.MovementType == ScoreMovementTypeConstants.AbandonPenalty)
                    .Sum(m => m.Points);

                int penaltyCount = movements
                    .Count(m => m.MovementType == ScoreMovementTypeConstants.AbandonPenalty);

                List<ScoreMovementDto> movementDtos = movements
                    .Select(ScoreMapper.ToScoreMovementDto)
                    .ToList();

                return ScoreResponseFactory.BuildGetPlayerScoreResponse(
                    true,
                    ScoreMessageCode.Success,
                    totalScore,
                    guesserWinTotal,
                    guesserWinCount,
                    hostWinTotal,
                    hostWinCount,
                    penaltyTotal,
                    penaltyCount,
                    movementDtos);
            }
        }
    }
}
