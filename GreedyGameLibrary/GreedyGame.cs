/** Author:     Vo, Dinh Tue Minh
 *  Date:       March 25, 2021
 *  Purpose:    Implement service contract for Greedy Game application
 */

using System;
using System.Collections.Generic;

using System.ServiceModel;

namespace GreedyGameLibrary
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class GreedyGame : IGreedyGame
    {
        // maximum number of players can join a game
        // has to modify the client UI if change this number
        private const int MAXIMUM_REQUIRED_PLAYERS = 7;
        // minimum number of players that can start a game
        // must be greater than 2
        private const int MINIMUM_REQUIRED_PLAYERS = 3;
        private const int MINIMUM_TARGET_SCORE = 10;
        private const int MAXIMUM_TARGET_SCORE = 20;

        private Client[] _clients;
        private Client _host;
        private int _targetScore;

        public GreedyGame()
        {
            _clients = new Client[MAXIMUM_REQUIRED_PLAYERS];
            _host = null;
            _targetScore = 0;
        }

        // ----------------------------------------------------------------------
        // Implementation of the IGreedyGame interface
        // ----------------------------------------------------------------------
        #region IGREEDY_INTERFACE_IMPLEMENTATION

        int IGreedyGame.MinimumRequiredPlayers { get { return MINIMUM_REQUIRED_PLAYERS; } }

        int IGreedyGame.MaximumRequiredPlayers { get { return MAXIMUM_REQUIRED_PLAYERS; } }

        int IGreedyGame.MinimumTargetScore { get { return MINIMUM_TARGET_SCORE; } }

        int IGreedyGame.MaximumTargetScore { get { return MAXIMUM_TARGET_SCORE; } }

        int IGreedyGame.NumberConnectedClients
        {
            get
            {
                return CountConnectedClients_();
            }
        }

        string IGreedyGame.HostName { get { return _host == null ? "" : _host.Player.Name; } }

        Tuple<JoinResponseCode, Player> IGreedyGame.Join(string name)
        {
            // cannot join when the game is already started
            if (IsGameStarted_()) return new Tuple<JoinResponseCode, Player>(JoinResponseCode.GameIsStarted, null);

            // check if name is invalid string
            string sanitizedName = SanitizeName_(name);
            if (sanitizedName == string.Empty) return new Tuple<JoinResponseCode, Player>(JoinResponseCode.NameIsInvalid, null);

            // check if the player name already exists
            var client = FindClientByName_(sanitizedName);
            if (client != null) return new Tuple<JoinResponseCode, Player>(JoinResponseCode.NameIsUsed, null); ;

            // check if there is an empty slot
            for (int i = 0; i < MAXIMUM_REQUIRED_PLAYERS; ++i)
            {
                if (_clients[i] == null || _clients[i].IsDisconnected())
                {
                    // retrieve and save client's callback proxy
                    ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();
                    _clients[i] = new Client(sanitizedName, cb);
                    Console.WriteLine($"Slot {i + 1}: {sanitizedName} has joined");

                    // update the clients
                    // - players dashboard
                    // - control panels on other players rather than host
                    Player[] players = GetPlayers_();
                    foreach (var c in _clients)
                    {
                        if (c != null && c.IsDisconnected() == false)
                        {
                            c.Callback.UpdatePlayersDashboard(players);
                            if (_host == null || c.Callback != _host.Callback)
                            {
                                c.Callback.UpdateControlPanels(_host == null, _targetScore);
                            }
                        }
                    }
                    // send the player object back to the client
                    return new Tuple<JoinResponseCode, Player>(JoinResponseCode.Accept, _clients[i].Player);
                }
            }
            return new Tuple<JoinResponseCode, Player>(JoinResponseCode.GameIsFull, null);
        }

        void IGreedyGame.Leave()
        {
            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();
            var client = FindClientByCallback_(cb);

            // remove the leaving client from the callback list
            client.Callback = null;

            Console.WriteLine($"{client.Player.Name} has left the game");
            if (IsGameStarted_())
            {
                // case: the game is already started
                //  - check if the number of remaining players is enough to continue the game
                //      - if yes:
                //          - update the player state but do not update the clients
                //          - check if the remaining players have already picked
                //              - if true: process the round
                //              - otherwise: continue
                //      - otherwise: reset the game
                int numberConnectedClients = CountConnectedClients_();
                if (numberConnectedClients >= MINIMUM_REQUIRED_PLAYERS)
                {
                    // reset everything but the score
                    client.Player.Status = "Disconnected";
                    client.Player.LastPick = 0;
                    client.Player.IsLastRoundWinner = false;
                    client.CurrentPick = 0;

                    if(numberConnectedClients == CountClientsHasPlayed_())
                    {
                        ProcessARound_();
                    }
                    else
                    {
                        ; // do nothing
                    }
                }
                else
                {
                    // reset the game
                    foreach (var c in _clients)
                    {
                        if (c != null)
                        {
                            // remove disconnect client from the list
                            if (c.IsDisconnected() == true)
                            {
                                c.Player = null;
                            }
                            else
                            {
                                c.Player.Status = "";
                                c.Player.LastPick = 0;
                                c.Player.IsLastRoundWinner = false;
                                c.CurrentPick = 0;
                            }
                        }
                    }

                    _targetScore = 0; // indicate that the game is finish
                    Console.WriteLine("Game has been reset for a new game");

                    Player[] players = GetPlayers_();
                    foreach (var c in _clients)
                    {
                        if (c != null && c.IsDisconnected() == false)
                        {
                            c.Callback.UpdateGameDashboard(players, "Not enough players to continue!");
                            c.Callback.UpdatePlayersDashboard(players);
                            c.Callback.UpdateControlPanels(true, 0);
                            c.Callback.UpdatePlaygroundPanel(0);
                        }
                    }
                }
            }
            else
            {
                // case: the game is not started yet
                //  - remove player info in client
                //  - check if leaving client is the host
                //      - if true: enable control panels in remaning clients
                //  - update players dashboard to reflect the disconnection

                client.Player = null;

                if (client == _host) // object comparison
                {
                    Console.WriteLine("The game has no host");
                    // enable control panels in remaining clients
                    foreach (var c in _clients)
                    {
                        if (c != null && c.IsDisconnected() == false)
                        {
                            c.Callback.UpdateControlPanels(true, _targetScore);
                        }
                    }
                    // reset host
                    _host = null;
                }

                // update players dashboard
                Player[] players = GetPlayers_();
                foreach (var c in _clients)
                {
                    if (c != null && c.IsDisconnected() == false)
                    {
                        c.Callback.UpdatePlayersDashboard(players);
                    }
                }
            }

        }

        bool IGreedyGame.BecomeHost()
        {
            if (_host != null && _host.Callback != null)
            {
                // there is a host
                return false;
            }

            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();
            // p should not be null
            var client = FindClientByCallback_(cb);

            _host = client;
            _host.Player.Status = "Host";

            Console.WriteLine($"{_host.Player.Name} has become host");

            // update clients
            Player[] players = GetPlayers_();
            foreach (var c in _clients)
            {
                if (c != null && c.IsDisconnected() == false)
                {
                    c.Callback.UpdatePlayersDashboard(players);
                    // update the control panels of other clients rather than host
                    if (_host == null || c.Callback != _host.Callback)
                    {
                        c.Callback.UpdateControlPanels(_host == null, _targetScore);
                    }
                }
            }

            return true;
        }

        bool IGreedyGame.Start(int score)
        {
            // must have at least some players to play
            if (CountConnectedClients_() >= MINIMUM_REQUIRED_PLAYERS)
            {

                ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();
                var client = FindClientByCallback_(cb);

                // sanity check if the request from host (probably not necessary)
                if (client == _host && score >= MINIMUM_TARGET_SCORE && score <= MAXIMUM_TARGET_SCORE) // reference comparison
                {
                    // update game state
                    SetGameStateToNewGame_(score);

                    // no need for host
                    _host = null;

                    // update clients
                    // - disable control panels
                    // - update players dashboard
                    // - update the playground panel
                    Player[] players = GetPlayers_();
                    int numberOptions = CalculateNumberOptions_();
                    Console.WriteLine($"Game has started with target score of {score} and number of options of {numberOptions}");
                    foreach (var c in _clients)
                    {
                        if (c != null && c.IsDisconnected() == false)
                        {
                            c.Callback.UpdatePlayersDashboard(players);
                            c.Callback.UpdateControlPanels(false, _targetScore);
                            c.Callback.UpdateGameDashboard(players, "Game started!");
                            c.Callback.UpdatePlaygroundPanel(numberOptions);
                        }
                    }

                    return true;
                }
            }
            return false;
        }

        void IGreedyGame.Play(int option)
        {
            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();
            var client = FindClientByCallback_(cb);
            Console.WriteLine($"{client.Player.Name} has picked Card #{option}");

            client.SetPick(option);

            // check if all clients have played
            // if true: process a round
            // otherwise:
            //  - update players dashboard to reflect who has made their selections.

            if (CountClientsHasPlayed_() == CountConnectedClients_())
            {
                ProcessARound_();
            }
            else
            {
                // update the other clients
                Player[] players = GetPlayers_();
                foreach (var c in _clients)
                {
                    if (c != null && c.IsDisconnected() == false)
                    {
                        c.Callback.UpdatePlayersDashboard(players);
                    }
                }
            }
        }

        #endregion IGREEDY_INTERFACE_IMPLEMENTATION

        // ----------------------------------------------------------------------
        // Helpers
        // ----------------------------------------------------------------------
        #region HELPERS

        private string SanitizeName_(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            char[] chars = name.Trim().ToLower().ToCharArray();
            chars[0] = char.ToUpper(chars[0]);
            return new string(chars);
        }

        private Player[] GetPlayers_()
        {
            List<Player> players = new List<Player>();
            foreach (var client in _clients)
            {
                players.Add(client == null ? null : client.Player);
            }
            return players.ToArray();
        }

        // name must be already sanitized
        private Client FindClientByName_(string name)
        {
            for (int i = 0; i < MAXIMUM_REQUIRED_PLAYERS; ++i)
            {
                if (_clients[i] != null && _clients[i].Player != null && _clients[i].Player.Name == name)
                {
                    return _clients[i];
                }
            }
            return null;
        }

        private Client FindClientByCallback_(ICallback cb)
        {
            for (int i = 0; i < MAXIMUM_REQUIRED_PLAYERS; ++i)
            {
                if (_clients[i] != null && _clients[i].Callback != null && _clients[i].Callback == cb)
                {
                    return _clients[i];
                }
            }
            return null;
        }

        private int CountConnectedClients_()
        {
            int count = 0;
            for (int i = 0; i < MAXIMUM_REQUIRED_PLAYERS; ++i)
            {
                if (_clients[i] != null && _clients[i].IsDisconnected() == false)
                {
                    ++count;
                }
            }
            return count;
        }

        private int CountClientsHasPlayed_()
        {
            int count = 0;
            for (int i = 0; i < MAXIMUM_REQUIRED_PLAYERS; ++i)
            {
                if (_clients[i] != null && _clients[i].IsDisconnected() == false && _clients[i].CurrentPick != 0)
                {
                    ++count;
                }
            }
            return count;
        }

        private int CalculateNumberOptions_(int numberClients)
        {
            return (int)Math.Ceiling(numberClients / 2.0);
        }

        // only run this method after all clients have played
        private int GetUniquePickAfterARound_()
        {
            int[] count = new int[CalculateNumberOptions_(MAXIMUM_REQUIRED_PLAYERS) + 1];
            foreach (var client in _clients)
            {
                if (client != null && client.IsDisconnected() == false)
                {
                    ++count[client.CurrentPick];
                }
            }
            for (int i = count.Length - 1; i >= 0; --i)
            {
                if (count[i] == 1)
                {
                    return i;
                }
            }
            return 0;
        }

        // Update the scores of players after a round
        // and return the winner if there is one
        private Player UpdatePlayersScore_()
        {
            int wonScore = GetUniquePickAfterARound_();
            Player winner = null;
            foreach (var client in _clients)
            {
                if (client != null && client.IsDisconnected() == false)
                {
                    client.UpdateScore(wonScore);
                    if (client.Player.IsLastRoundWinner)
                    {
                        winner = client.Player;
                    }
                }
            }
            return winner;
        }

        private void SetGameStateToNewGame_(int targetScore)
        {
            _targetScore = targetScore;
            foreach (var client in _clients)
            {
                if (client != null)
                {
                    if (client.IsDisconnected() == false)
                    {
                        client.CurrentPick = 0;
                        client.Player.LastPick = 0;
                        client.Player.IsLastRoundWinner = false;
                        client.Player.Score = 0;
                        client.Player.Status = "Selecting";
                    }
                    else
                    {
                        // remove disconnected clients from the list
                        client.Player = null;
                    }
                }
            }
        }

        private int GetHighestScore_()
        {
            int highestScore = 0;
            foreach (var client in _clients)
            {
                if (client != null && client.IsDisconnected() == false)
                {
                    highestScore = highestScore > client.Player.Score ? highestScore : client.Player.Score;
                }
            }
            return highestScore;
        }

        private int CalculateNumberOptions_()
        {
            return (int)Math.Ceiling(CountConnectedClients_() / 2.0);
        }

        private bool IsGameStarted_()
        {
            return _targetScore > 0;
        }

        private void ProcessARound_()
        {
            //  - update all players' score
            //  - check if the game is finish:
            //      - if true:
            //          - update game state to post game
            //          - update game dashboard + status label to reflect the game winner
            //          - update players dashboard
            //          - update control panels to let players start a new game
            //          - disable the playground
            //      - otherwise:
            //          - update game dashboard + status label to reflect the round winner (if there is one)
            //          - update players dashboard
            //          - update playground panel to let players start next round

            Player winner = UpdatePlayersScore_();

            if (GetHighestScore_() >= _targetScore)
            {
                foreach (var c in _clients)
                {
                    if (c != null)
                    {
                        // remove disconnect client from the list
                        if (c.IsDisconnected() == true)
                        {
                            c.Player = null;
                        }
                        else
                        {
                            c.Player.Status = "";
                            c.Player.LastPick = 0;
                        }
                    }
                }

                winner.Status = "Winner";
                string status = $"{winner.Name} won last game!";
                Console.WriteLine(status);

                _targetScore = 0; // indicate that the game is finish
                Console.WriteLine("Game has been reset for a new game");

                Player[] players = GetPlayers_();
                foreach (var c in _clients)
                {
                    if (c != null && c.IsDisconnected() == false)
                    {
                        c.Callback.UpdateGameDashboard(players, status);
                        c.Callback.UpdatePlayersDashboard(players);
                        c.Callback.UpdateControlPanels(true, 0);
                        c.Callback.UpdatePlaygroundPanel(0);
                    }
                }
            }
            else
            {
                Player[] players = GetPlayers_();
                string status = winner == null ? "No one won last round!" : $"{winner.Name} won last round!";
                if (winner == null)
                {
                    Console.WriteLine("No one won last round");
                }
                int numberOptions = CalculateNumberOptions_();
                foreach (var c in _clients)
                {
                    if (c != null && c.IsDisconnected() == false)
                    {
                        c.Callback.UpdateGameDashboard(players, status);
                        c.Callback.UpdatePlayersDashboard(players);
                        c.Callback.UpdatePlaygroundPanel(numberOptions);
                    }
                }
            }
        }

        #endregion HELPERS
    
    }
}
