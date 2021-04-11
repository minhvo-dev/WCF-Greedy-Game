/** Author:     Vo, Dinh Tue Minh
 *  Date:       March 25, 2021
 *  Purpose:    Data contract for Greedy Game application
 */

using System.Runtime.Serialization;

namespace GreedyGameLibrary
{
    [DataContract]
    public class Player
    {
        [DataMember]
        public string Name { get; internal set; }

        [DataMember]
        public int LastPick { get; internal set; }

        [DataMember]
        public string Status { get; internal set; }

        [DataMember]
        public int Score { get; internal set; }

        [DataMember]
        public bool IsLastRoundWinner { get; internal set; }

        internal Player(string name)
        {
            Name = name;
            LastPick = 0;
            Status = "";
            Score = 0;
            IsLastRoundWinner = false;
        }
    }
}
