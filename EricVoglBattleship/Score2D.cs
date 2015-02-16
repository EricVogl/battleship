using System.Collections.Generic;
using BattleshipUtility;

namespace EricVoglBattleship
{
    /// <summary>
    /// Keeps track of the number of ship position possibilities in two dimensions
    /// </summary>
    public class Score2D
    {
        #region Fields        

        /// <summary>
        /// Number of ship possibilities in the horizontal (row) dimension
        /// </summary>
        Score1D _horizontalScore = new Score1D();

        /// <summary>
        /// Number of ship possibilities in the vertical (column) dimension
        /// </summary>
        Score1D _verticalScore = new Score1D();

        /// <summary>
        /// Total number of ship position possibilities      
        /// </summary>
        /// <remarks>This is updated as individual dimension/ship scores change</remarks>
        int _totalScore = 0;

        #endregion

        #region Properties        

        /// <summary>
        /// Total number of ship position possibilities at this position
        /// </summary>
        public int Score
        {
            get
            {
                return _totalScore;
            }            
        }

        #endregion

        #region Methods        

        /// <summary>
        /// Forces a recalculation of the total score
        /// </summary>
        public void Recalculate()
        {
            _totalScore = 0;
            _horizontalScore.Recalculate();
            _verticalScore.Recalculate();
            _totalScore = _horizontalScore.Score + _verticalScore.Score;
        }

        /// <summary>
        /// Adjusts score based on the fact that a ship has sunk
        /// </summary>
        /// <param name="size">The size of ship that sank</param>
        public void Sink(int size)
        {
            _horizontalScore.Sink(size);
            _verticalScore.Sink(size);
            _totalScore = _horizontalScore.Score + _verticalScore.Score;
        }
        
        /// <summary>
        /// Applies a score to the horizontal dimension
        /// </summary>
        /// <param name="ships">ships alive</param>
        /// <param name="index">index within length</param>
        /// <param name="length">length</param>
        public void ApplyHorizontalScore(List<Ship> ships, int index, int length)
        {
            _horizontalScore.Apply(ships, index, length);
            _totalScore = _horizontalScore.Score + _verticalScore.Score;
        }

        /// <summary>
        /// Applies a score to the vertical dimension
        /// </summary>
        /// <param name="ships">ships alive</param>
        /// <param name="index">index within length</param>
        /// <param name="length">length</param>
        public void ApplyVerticalScore(List<Ship> ships, int index, int length)
        {
            _verticalScore.Apply(ships, index, length);
            _totalScore = _horizontalScore.Score + _verticalScore.Score;
        }

        /// <summary>
        /// Initializes the horizontal dimension
        /// </summary>
        /// <param name="ships">ships in game</param>
        /// <param name="index">index within length</param>
        /// <param name="length">length</param>
        public void InitializeHorizontalScore(List<Ship> ships, int index, int length)
        {
            _horizontalScore.Initialize(ships, index, length);
            _totalScore = _horizontalScore.Score + _verticalScore.Score;
        }

        /// <summary>
        /// Initializes the vertical dimension
        /// </summary>
        /// <param name="ships">ships in game</param>
        /// <param name="index">index within length</param>
        /// <param name="length">length</param>
        public void InitializeVerticalScore(List<Ship> ships, int index, int length)
        {
            _verticalScore.Initialize(ships, index, length);
            _totalScore = _horizontalScore.Score + _verticalScore.Score;
        }

        /// <summary>
        /// Clears the score
        /// </summary>
        public void Clear()
        {
            _horizontalScore.Clear();
            _verticalScore.Clear();
            _totalScore = 0;
        }

        #endregion
    }
}
