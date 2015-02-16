using System;
using System.Collections.Generic;
using BattleshipUtility;

namespace EricVoglBattleship
{
    /// <summary>
    /// Utility methods
    /// </summary>
    public static class BattleshipStatisticalUtility
    {        
        #region Static Methods        

        /// <summary>
        /// Populates a matrix with number of ship position possibilities in each row/column
        /// </summary>
        /// <param name="width">Width of board</param>
        /// <param name="height">Height of board</param>
        /// <param name="ships">Ships in play</param>
        /// <param name="totalPositions">Total position possibilities</param>
        /// <returns>new matrix primed with possibilities</returns>
        public static int[,] ShipPossibilityMatrix(int width, int height, List<Ship> ships, out int totalPositions)
        {
            int[,] matrix = new int[width, height];
            totalPositions = 0;

            // Prime the shots matrix with the number of different ship positions that are possible at a given location
            for (var i = 0; i < height; ++i)
            {
                for (var j = 0; j < width; ++j)
                {
                    foreach (Ship s in ships)
                    {
                        matrix[i, j] += PositionPossibilities(i, height, s.Size);
                        matrix[i, j] += PositionPossibilities(j, width, s.Size);
                        totalPositions += matrix[i, j];
                    }
                }
            }

            return matrix;
        }

        /// <summary>
        /// Populates a matrix with number of ship position possibilities in each row/column (divided by ship type and orientation)
        /// </summary>
        /// <param name="width">Width of board</param>
        /// <param name="height">Height of board</param>
        /// <param name="ships">Ships in play</param>
        /// <returns>new matrix primed with possibilities</returns>
        public static Score2D[,] ShipPossibilityMatrix(int width, int height, List<Ship> ships)
        {
            Score2D[,] matrix = new Score2D[width, height];            

            // Prime the shots matrix with the number of different ship positions that are possible at a given location
            for (var i = 0; i < height; ++i)
            {
                for (var j = 0; j < width; ++j)
                {
                    matrix[i, j] = new Score2D();
                    matrix[i, j].InitializeVerticalScore(ships, i, height);
                    matrix[i, j].InitializeHorizontalScore(ships, j, width);
                }
            }

            return matrix;
        }

        /// <summary>
        /// Determines the number of possible ship orientations of a given size in a given 1D space at a given position in that space
        /// </summary>
        /// <param name="index">position in 1D space</param>
        /// <param name="length">size of 1D space</param>
        /// <param name="ships">ships in 1D space</param>
        /// <returns></returns>
        public static int PositionPossibilities(int index, int length, List<Ship> ships)
        {
            int total = 0;
            foreach (Ship ship in ships)
            {
                total += PositionPossibilities(index, length, ship.Size);
            }
            return total;
        }

        /// <summary>
        /// Determines the number of possible ship orientations of a given size in a given 1D space at a given position in that space
        /// </summary>
        /// <param name="index">position in 1D space</param>
        /// <param name="length">size of 1D space</param>
        /// <param name="ships">size of ship</param>
        public static int PositionPossibilities(int index, int length, int size)
        {
            return Math.Min(Math.Min(index + 1, length - index), (length - size < 0 ?  0 : (length - size == 0 ? 1 : size)));
        }        

        #endregion
    }
}
