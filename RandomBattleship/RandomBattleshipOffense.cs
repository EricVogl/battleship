using System.Collections.Generic;
using BattleshipUtility;

namespace RandomBattleship
{
    /// <summary>
    /// Random offense that shoots randomly, but will continue to attempt to sink
    /// </summary>
    public sealed class RandomBattleshipOffense : IBattleshipOffense
    {        
        #region Fields

        /// <summary>
        /// Keeps track of shots
        /// </summary>
        bool[,] _shotAt;                        

        #endregion

        #region Utility Methods
        
        /// <summary>
        /// Choose a random shot
        /// </summary>
        /// <returns>The position to shoot at</returns>
        Position RandomShot()
        {            
            bool found = false;
            int limit = 0;

            // Keep looking until we've checked every square or found a place we haven't shot
            while (!found && limit < Player.Width * Player.Height)
            {
                // Choose a random location
                int row = BattleshipGame.Random.Next(Player.Height);
                int col = BattleshipGame.Random.Next(Player.Width);

                // If we haven't shot at the location yet, shoot at it
                if (!_shotAt[row, col])
                {
                    return new Position(row, col);                    
                }

                limit++;
            }

            // We should never hit this
            return new Position(0, 0);
        }        
        
        #endregion

        #region IBattleshipOffense Implmentation

        /// <summary>
        /// Owning player
        /// </summary>
        public BattleshipPlayer Player { get; private set; }
        
        /// <summary>
        /// Initializes a battleship offense
        /// </summary>
        /// <param name="player">owning player</param>  
        public void Initialize(BattleshipPlayer player)
        {            
            Player = player;
            _shotAt = new bool[Player.Height, Player.Width];
        }

        /// <summary>
        /// Fires at a position
        /// </summary>
        /// <returns>Position to fire at</returns>
        public Position Fire()
        {
            Position p = RandomShot();
            _shotAt[p.Row, p.Column] = true;

            return p;
        }

        /// <summary>
        /// Called when a miss occurs
        /// </summary>
        public void Miss()
        {
            // Random player just doesn't care.
        }
        
        /// <summary>
        /// Called when a hit occurs
        /// </summary>
        public void Hit()
        {
            // Random player doesn't care at all
        }

        /// <summary>
        /// Called when a ship is sunk
        /// </summary>
        /// <param name="s">Code representing the ship</param>
        public void Sink(string s)
        {
            // Random player doesn't care
        }

        /// <summary>
        /// Enemy positions sent to us after game ends
        /// </summary>
        /// <param name="_ships">Positions of enemy ships</param>
        public void RegisterEnemyPositions(List<Ship> ships)
        {          
            // Random player doesn't care
        }

        /// <summary>
        /// Cleans up an offense
        /// </summary>
        /// <param name="state"></param>
        public void Cleanup(GameEndState state)
        {
            // Random player didn't leave a mess
        }

        #endregion
    }
}
