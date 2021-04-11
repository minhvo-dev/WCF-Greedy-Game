/** Author:     Vo, Dinh Tue Minh
 *  Date:       March 25, 2021
 *  Purpose:    Internal class that keep track of the client info
 *              such as player info, callback proxy
 *              and hide the selection of the player from the others
 */

using System;

namespace GreedyGameLibrary
{
    internal class Client
    {
        public Player Player { get; internal set; }
        public int CurrentPick { get; internal set; } // hide this from data sent to client
        public ICallback Callback { get; internal set; }

        public Client(string name, ICallback callback)
        {
            Player = new Player(name);
            Callback = callback;
            CurrentPick = 0;
        }


        public void UpdateScore(int wonScore)
        {
            if (Player != null)
            {
                // if currentPick is equal to wonScore then update the score of the player
                Player.IsLastRoundWinner = false;
                if (CurrentPick == wonScore && wonScore != 0)
                {
                    // wonScore can be 0 when there is no winner
                    Player.Score += wonScore;
                    Player.IsLastRoundWinner = true;
                    Console.WriteLine($"{Player.Name} won last round for picking Card #{wonScore}");
                }
                Player.LastPick = CurrentPick;
                CurrentPick = 0;
                Player.Status = "Selecting";
            }
        }

        public bool IsDisconnected()
        {
            return Callback == null;
        }

        public void SetPick(int option)
        {
            CurrentPick = option;
            Player.Status = "Selected";
        }
    }
}
