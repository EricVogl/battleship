using System;
using System.Collections.Generic;
using System.Linq;
using BattleshipUtility;

namespace EricVoglBattleship
{
    /// <summary>
    /// Battleship defense that attempts to place ships in the least probable locations based on ship position possibilities and previous incoming shots
    /// </summary>
    public sealed class AdaptiveBattleshipDefense : IBattleshipDefense
    {
        #region Static Fields
        
        /// <summary>
        /// Number of samples to use when generating possible ship positions via Monte Carlo
        /// </summary>
        static int _samples = 1000;

        #endregion

        #region Fields
        
        /// <summary>
        /// Possible ship positions
        /// </summary>
        int[,] _positionProbability;

        /// <summary>
        /// Incoming shot matrix, recording turn shot was fired.  This turn value is adjusted later so that earlier turns are weighted heavier
        /// </summary>
        int[,] _incomingShots;

        /// <summary>
        /// Total number of possible positions
        /// </summary>
        int _totalPositions;        
                
        #endregion

        #region Properties

        /// <summary>
        /// Owning player (as an adaptive player)
        /// </summary>
        public AdaptiveBattleshipPlayer AdaptivePlayer { get; private set; }

        #endregion

        #region IBattleshipDefense Implementation

        /// <summary>
        /// Owning Player
        /// </summary>
        public BattleshipPlayer Player { get { return AdaptivePlayer; } }

        /// <summary>
        /// Initializes a battleship defense
        /// </summary>
        /// <param name="player">owning player</param>
        public void Initialize(BattleshipPlayer player)
        {
            AdaptivePlayer = player as AdaptiveBattleshipPlayer;
            
            _positionProbability = BattleshipStatisticalUtility.ShipPossibilityMatrix(AdaptivePlayer.Width, AdaptivePlayer.Height, AdaptivePlayer.Ships, out _totalPositions);

            // If we haven't played this player before, initialize the shot matrix with the position probability matrix
            if (AdaptivePlayer.Data.Wins + AdaptivePlayer.Data.Losses + AdaptivePlayer.Data.Ties == 0)
            {
                for(var i = 0; i < AdaptivePlayer.Height; ++i)
                {
                    for(var j = 0; j < AdaptivePlayer.Width; ++j)
                    {
                        AdaptivePlayer.Data.IncomingShots[i][j] = _positionProbability[i, j];
                    }
                }
            }
            else
            {
                _totalPositions = 0;
                for(var i = 0; i < AdaptivePlayer.Height; ++i)
                {
                    for(var j = 0; j < AdaptivePlayer.Width; ++j)
                        _totalPositions += AdaptivePlayer.Data.IncomingShots[i][j];
                }
            }

            _incomingShots = new int[AdaptivePlayer.Width, AdaptivePlayer.Height];            
        }

        /// <summary>
        /// Register an incoming shot
        /// </summary>
        /// <param name="p">Position shot at</param>
        public void IncomingShot(Position p)
        {
            _incomingShots[p.Row, p.Column] = AdaptivePlayer.Turn;            
        }

        /// <summary>
        /// Place ships on board
        /// </summary>
        /// <returns>List of ship placements</returns>
        public List<Ship> PlaceShips()
        {
            // We want to be both random and smart
            // Generate a series of positions, score them based on position probability and opponent shooting data
            List<Ship> placedShips = new List<Ship>(AdaptivePlayer.Ships.Count);
            List<Ship> bestShipPlacement = new List<Ship>(AdaptivePlayer.Ships.Count);
            double minimumScore = double.MaxValue;
            bool allowTouching = true;

            // Randomly decide to disallow touching ships
            if (BattleshipGame.Random.NextDouble() < 0.6d)
            {
                allowTouching = false;
            }

            // Randomly choose 1000 different layouts, choose the layout with the lowest score
            for (int sample = 0; sample < _samples; ++sample)
            {
                double currentScore = 0.0d;
                placedShips.Clear();
                foreach (Ship ship in AdaptivePlayer.Ships)
                {
                    bool intersects = false;
                    Ship placedShip = new Ship() { Size = ship.Size, Code = ship.Code };

                    // Keep generating positions until we have a set that has no intersecting ships
                    do
                    {
                        intersects = false;
                        placedShip.Place(AdaptivePlayer.Width, AdaptivePlayer.Height);
                        foreach (Ship checkShip in placedShips)
                        {
                            if (allowTouching)
                            {
                                if (placedShip.Intersects(checkShip))
                                {
                                    intersects = true;
                                    break;
                                }
                            }
                            else 
                            {
                                if (placedShip.IntersectsOrAdjacent(checkShip))
                                {
                                    intersects = true;
                                    break;
                                }
                            }
                        }
                    } while (intersects);

                    // Score the positions of each ship
                    foreach(Position p in placedShip.Positions)
                    {
                        // Default to probability distribution, otherwise use previous shot history
                        currentScore += (double)AdaptivePlayer.Data.IncomingShots[p.Row][p.Column] / (double)_totalPositions;
                    }
                    placedShips.Add(placedShip);
                }

                if (currentScore < minimumScore)
                {
                    minimumScore = currentScore;
                    bestShipPlacement.Clear();
                    foreach (Ship ship in placedShips)
                        bestShipPlacement.Add(ship);
                }
            }

            // We want to avoid using this placement again, add some points to the current location
            int totalShipSize = bestShipPlacement.Sum(x => x.Size);
            foreach (Ship ship in bestShipPlacement)
            {
                foreach (Position p in ship.Positions)
                {
                    AdaptivePlayer.Data.IncomingShots[p.Row][p.Column] += (int)((double)ship.Size / (double)totalShipSize * 1000.0d);
                }
            }

            return bestShipPlacement;
        }

        /// <summary>
        /// Cleans up a battleship defense
        /// </summary>
        /// <param name="state">Win/Lose/Draw</param>
        public void Cleanup(GameEndState state)
        {
            // Record defense data
            for (int i = 0; i < AdaptivePlayer.Height; ++i)
            {
                for (int j = 0; j < AdaptivePlayer.Width; ++j)
                {
                    // If we were shot at, weigh earlier shots heavier than later shots
                    if (_incomingShots[i, j] > 0)
                    {
                        AdaptivePlayer.Data.IncomingShots[i][j] += (int)((double)(AdaptivePlayer.Turn - _incomingShots[i, j] + 1) / (double)AdaptivePlayer.Turn * 1000.0d);
                    }
                }
            }
        }

        #endregion
    }
}
