using System.Collections.Generic;

namespace BattleshipUtility
{
    /// <summary>
    /// Contract describing a battleship defense
    /// </summary>
    public interface IBattleshipDefense
    {
        /// <summary>
        /// Owning player
        /// </summary>
        BattleshipPlayer Player { get; }

        /// <summary>
        /// Initializes a battleship defense
        /// </summary>
        /// <param name="player">owning player</param>       
        void Initialize(BattleshipPlayer player);

        /// <summary>
        /// Register an incoming shot
        /// </summary>
        /// <param name="p">Position shot at</param>
        void IncomingShot(Position p);

        /// <summary>
        /// Place ships on board
        /// </summary>
        /// <returns>List of ship placements</returns>
        List<Ship> PlaceShips();

        /// <summary>
        /// Cleans up a battleship defense
        /// </summary>
        /// <param name="state">Win/Lose/Draw</param>
        void Cleanup(GameEndState state);
    }
}
