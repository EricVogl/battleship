namespace EricVoglBattleship
{   
    /// <summary>
    /// Data stored about an opponent
    /// </summary>
    public class BattleshipData
    {       
        /// <summary>
        /// Incoming shot scores
        /// </summary>
        public int[][] IncomingShots { get; set; }        

        /// <summary>
        /// Outgoing hit scores
        /// </summary>
        public int[][] OutgoingHits { get; set; }

        /// <summary>
        /// Outgoing miss scores
        /// </summary>
        public int[][] OutgoingMisses { get; set; }

        /// <summary>
        /// Number of times opponent has allowed adjacent hits
        /// </summary>
        public int AllowsAdjacent { get; set; }        

        /// <summary>
        /// Minimum turns vs opponent
        /// </summary>
        public int MinTurns { get; set; }

        /// <summary>
        /// Maximum turns vs opponent
        /// </summary>
        public int MaxTurns { get; set; }

        /// <summary>
        /// Average turns vs opponent
        /// </summary>
        public float AverageTurns { get; set; }

        /// <summary>
        /// Wins
        /// </summary>
        public int Wins { get; set; }

        /// <summary>
        /// Losses
        /// </summary>
        public int Losses { get; set; }

        /// <summary>
        /// Ties
        /// </summary>
        public int Ties { get; set; }
    }
}
