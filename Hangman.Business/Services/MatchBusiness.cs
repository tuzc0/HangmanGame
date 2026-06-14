using Hangman.Business.Factories;
using Hangman.Business.Interfaces;
using Hangman.Business.UserCases.Match;
using Hangman.Contracts.Match;
using System;
using System.Threading.Tasks;

namespace Hangman.Business.Services
{
    public class MatchBusiness : IMatchBusiness
    {
        private readonly CreateLobbyUseCase createLobbyUseCase;
        private readonly GetAvailableLobbiesUseCase getAvailableLobbiesUseCase;
        private readonly JoinLobbyUseCase joinLobbyUseCase;
        private readonly GetCurrentLobbyUseCase getCurrentLobbyUseCase;
        private readonly LeaveLobbyUseCase leaveLobbyUseCase;

        public MatchBusiness(IUnitOfWorkFactory unitOfWorkFactory)
        {
            if (unitOfWorkFactory == null)
            {
                throw new ArgumentNullException(nameof(unitOfWorkFactory));
            }

            createLobbyUseCase = new CreateLobbyUseCase(unitOfWorkFactory);
            getAvailableLobbiesUseCase = new GetAvailableLobbiesUseCase(unitOfWorkFactory);
            joinLobbyUseCase = new JoinLobbyUseCase(unitOfWorkFactory);
            getCurrentLobbyUseCase = new GetCurrentLobbyUseCase(unitOfWorkFactory);
            leaveLobbyUseCase = new LeaveLobbyUseCase(unitOfWorkFactory);
        }

        public Task<CreateLobbyResponse> CreateLobbyAsync(CreateLobbyRequest request)
        {
            return createLobbyUseCase.ExecuteAsync(request);
        }

        public Task<GetAvailableLobbiesResponse> GetAvailableLobbiesAsync(
            GetAvailableLobbiesRequest request)
        {
            return getAvailableLobbiesUseCase.ExecuteAsync(request);
        }

        public Task<JoinLobbyResponse> JoinLobbyAsync(JoinLobbyRequest request)
        {
            return joinLobbyUseCase.ExecuteAsync(request);
        }

        public Task<GetCurrentLobbyResponse> GetCurrentLobbyAsync(
            GetCurrentLobbyRequest request)
        {
            return getCurrentLobbyUseCase.ExecuteAsync(request);
        }

        public Task<LeaveLobbyResponse> LeaveLobbyAsync(LeaveLobbyRequest request)
        {
            return leaveLobbyUseCase.ExecuteAsync(request);
        }
    }
}
