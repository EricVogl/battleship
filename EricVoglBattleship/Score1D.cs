using System.Collections.Generic;
using System.Linq;
using BattleshipUtility;

namespace EricVoglBattleship
{    
    /// <summary>
    /// Keeps track of the number of ship position possibilities in one dimension
    /// </summary>
    public class Score1D
    {       
        #region Fields        

        /// <summary>
        /// List containing ship position possibilities
        /// </summary>
        /// <remarks>We break this down by ship so we can quickly update when a ship sinks</remarks>
        List<ShipPossibility> _positionPossibilities;

        /// <summary>
        /// The total 'score' is the sum of ship position possibilities in one dimension (row or column)
        /// </summary>
        /// <remarks>This is updated as individual ship scores change</remarks>
        int _totalScore = 0;

        #endregion

        #region Constructors        

        /// <summary>
        /// Creates a new Score1D object
        /// </summary>
        public Score1D()
        {
            Clear();
        }

        #endregion

        #region Properties        

        /// <summary>
        /// Total number of possible ship positions in one dimension
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
            for (var i = 0; i < _positionPossibilities.Count; ++i)
                _totalScore += _positionPossibilities[i].Possibilities;
        }       

        /// <summary>
        /// Applies possibilities in 1D
        /// </summary>
        /// <param name="ships">Ships alive</param>
        /// <param name="index">index in length</param>
        /// <param name="length">length</param>
        public void Apply(List<Ship> ships, int index, int length)
        {
            _totalScore = 0;
            _positionPossibilities.ForEach(x => x.Processed = false);

            foreach(Ship ship in ships)
            {                
                foreach (ShipPossibility possibility in _positionPossibilities.Where(x => x.Size == ship.Size && !x.Processed))
                {
                    if (possibility.Possibilities > 0)
                    {
                        possibility.Possibilities = BattleshipStatisticalUtility.PositionPossibilities(index, length, ship.Size);                        
                        _totalScore += possibility.Possibilities;
                    }
                    possibility.Processed = true;
                }
            }
        }

        /// <summary>
        /// Initializes possibilities in 1D
        /// </summary>
        /// <param name="ships">Ships alive</param>
        /// <param name="index">index in length</param>
        /// <param name="length">length</param>
        public void Initialize(List<Ship> ships, int index, int length)
        {
            _positionPossibilities = new List<ShipPossibility>(ships.Count);
            _totalScore = 0;
            foreach (Ship s in ships)
            {
                ShipPossibility possibility = new ShipPossibility();
                possibility.Possibilities = BattleshipStatisticalUtility.PositionPossibilities(index, length, s.Size);
                possibility.Size = s.Size;
                _positionPossibilities.Add(possibility);
                _totalScore += possibility.Possibilities;
            }            
        }

        /// <summary>
        /// Sets the score to zero
        /// </summary>
        public void Clear()
        {
            _totalScore = 0;
            if (_positionPossibilities != null)
                _positionPossibilities.Clear();            
        }

        /// <summary>
        /// Update possible positions knowning ship sank
        /// </summary>
        /// <param name="size">Size of ship that sank</param>
        public void Sink(int size)
        {
            ShipPossibility remove = null;

            _totalScore = 0;
            foreach (ShipPossibility possibility in _positionPossibilities)
            {
                if ((possibility.Size == size) && (remove == null))
                {
                    remove = possibility;                    
                } 
                else 
                {
                    _totalScore += possibility.Possibilities;
                }
            }

            if (remove != null)
            {
                _positionPossibilities.Remove(remove);
            }
        }

        #endregion
    }
}
