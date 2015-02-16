using System.Collections.Generic;

namespace BattleshipUtility
{
    /// <summary>
    /// Contract describing a battleship offense
    /// </summary>
    public interface IBattleshipOffense
    {
        /// <summary>
        /// Owning player
        /// </summary>
        BattleshipPlayer Player { get; }
        
        /// <summary>
        /// Initializes a battleship offense
        /// </summary>
        /// <param name="player">owning player</param>       
        void Initialize(BattleshipPlayer player);

        /// <summary>
        /// Fires at a position
        /// </summary>
        /// <returns>Position to fire at</returns>
        Position Fire();

        /// <summary>
        /// Register last fire as a miss
        /// </summary>
        void Miss();

        /// <summary>
        /// Register last fire as a hit
        /// </summary>
        void Hit();

        /// <summary>
        /// Notify that a ship has sunk
        /// </summary>
        /// <param name="s">Code representing the ship</param>
        void Sink(string s);

        /// <summary>
        /// Enemy positions sent to us after game ends
        /// </summary>
        /// <param name="_ships">Positions of enemy ships</param>
        void RegisterEnemyPositions(List<Ship> _ships);

        /// <summary>
        /// Cleans up an offense
        /// </summary>
        /// <param name="state"></param>
        void Cleanup(GameEndState state);
    }
}
