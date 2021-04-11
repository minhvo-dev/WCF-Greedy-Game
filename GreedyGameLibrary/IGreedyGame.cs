/** Author:     Vo, Dinh Tue Minh
 *  Date:       March 25, 2021
 *  Purpose:    Define the service contract interface for Greedy Game application
 */

using System;

using System.ServiceModel;

namespace GreedyGameLibrary
{
    public enum JoinResponseCode
    {
        Accept = 0,
        NameIsInvalid,
        NameIsUsed,
        GameIsFull,
        GameIsStarted
    }

    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface IGreedyGame
    {
        int MinimumRequiredPlayers { [OperationContract] get; }

        int MaximumRequiredPlayers { [OperationContract] get; }

        int MinimumTargetScore { [OperationContract] get; }

        int MaximumTargetScore { [OperationContract] get; }

        int NumberConnectedClients { [OperationContract] get; }

        string HostName { [OperationContract] get; }

        // Call when a client want to join a game
        [OperationContract]
        Tuple<JoinResponseCode, Player> Join(string name);

        // Call when a client leaves the game
        [OperationContract(IsOneWay = true)]
        void Leave();

        // Call when a client want to be the host
        [OperationContract]
        bool BecomeHost();

        // Call when the host want to start the game
        [OperationContract]
        bool Start(int score);

        // Call when a client play a card
        [OperationContract(IsOneWay = true)]
        void Play(int option);
    }
}
