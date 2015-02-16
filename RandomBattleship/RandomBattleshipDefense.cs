using System.Collections.Generic;
using BattleshipUtility;

namespace RandomBattleship
{
    /// <summary>
    /// Battleship defense that places ships randomly (no overlap)
    /// </summary>
    public sealed class RandomBattleshipDefense : IBattleshipDefense
    {                
        #region IBattleshipDefense Implementation

        /// <summary>
        /// Owning player
        /// </summary>
        public BattleshipPlayer Player { get; private set; }

        /// <summary>
        /// Initializes a battleship defense
        /// </summary>
        /// <param name="player">owning player</param>
        public void Initialize(BattleshipPlayer player)
        {
            Player = player;                 
        }

        /// <summary>
        /// Register an incoming shot
        /// </summary>
        /// <param name="p">Position shot at</param>
        public void IncomingShot(Position p)
        {     
            // Random player doesn't even want to know
        }

        /// <summary>
        /// Place ships on board
        /// </summary>
        /// <returns>List of ship placements</returns>
        public List<Ship> PlaceShips()
        {
            // List of ships that were randomly placed
            List<Ship> placedShips = new List<Ship>(Player.Ships.Count);            

            // Randomly choose a layout            
            foreach (Ship ship in Player.Ships)
            {
                bool intersects = false;
                Ship placedShip = new Ship() { Size = ship.Size, Code = ship.Code };

                // Keep generating positions until we have a set that has no intersecting ships
                do
                {
                    intersects = false;
                    placedShip.Place(Player.Width, Player.Height);
                    foreach (Ship checkShip in placedShips)
                    {

                        if (placedShip.IntersectsOrAdjacent(checkShip))
                        {
                            intersects = true;
                            break;
                        }

                    }
                } while (intersects);

                placedShips.Add(placedShip);
            }

            return placedShips;
        }

        /// <summary>
        /// Cleans up a battleship defense
        /// </summary>
        /// <param name="state">Win/Lose/Draw</param>
        public void Cleanup(GameEndState state)
        {            
        }

        #endregion
    }
}
