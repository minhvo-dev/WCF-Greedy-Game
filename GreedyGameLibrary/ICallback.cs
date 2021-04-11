/** Author:     Vo, Dinh Tue Minh
 *  Date:       March 25, 2021
 *  Purpose:    Implement service contract for Greedy Game application
 */

using System.ServiceModel;

namespace GreedyGameLibrary
{
    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void UpdatePlayersDashboard(Player[] players);

        [OperationContract(IsOneWay = true)]
        void UpdateControlPanels(bool enableHost, int targetScore);

        [OperationContract(IsOneWay = true)]
        void UpdateGameDashboard(Player[] players, string status);

        [OperationContract(IsOneWay = true)]
        void UpdatePlaygroundPanel(int numberOptions);
    }
}
