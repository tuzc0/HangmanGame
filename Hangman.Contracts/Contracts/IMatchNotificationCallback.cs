using System.ServiceModel;

namespace Hangman.Contracts.Contracts
{
    public interface IMatchNotificationCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnLobbyUpdated(int matchId);

        [OperationContract(IsOneWay = true)]
        void OnLobbyClosed(int matchId, string messageCode);

        [OperationContract(IsOneWay = true)]
        void OnMatchStatusChanged(int matchId, string matchStatus);

        [OperationContract(IsOneWay = true)]
        void OnAvailableLobbiesChanged();
    }
}
