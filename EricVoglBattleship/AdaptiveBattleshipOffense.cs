using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using BattleshipUtility;

namespace EricVoglBattleship
{
    /// <summary>
    /// Battleship offense that attempts to shoot at the most probable location on the board, given possible ship positions and previous placement history
    /// </summary>
    public sealed class AdaptiveBattleshipOffense : IBattleshipOffense
    {
        #region Static Fields

        /// <summary>
        /// Smoothing factor number of simulated hits
        /// </summary>
        static int _laplaceSmoothHits = 0;

        /// <summary>
        /// Smoothing factor total shots
        /// </summary>
        readonly static int _laplaceSmoothFactor = 15;
     
        #endregion

        #region Fields

        /// <summary>
        /// Matrix storing possible ship positions
        /// </summary>
        Score2D[,] _score;

        /// <summary>
        /// Score matrix assuming the player does not allow adjacent ships
        /// </summary>
        Score2D[,] _notAdjacentScore;

        /// <summary>
        /// Last shot fired at
        /// </summary>
        Position _lastShot = Position.Empty;

        /// <summary>
        /// Positions where pending hits have been registered (but not all sunk)
        /// </summary>
        List<Position> _hits = new List<Position>();

        /// <summary>
        /// All hits that we've registered
        /// </summary>
        List<Position> _allHits = new List<Position>();

        /// <summary>
        /// Which way are we trying to sink a ship
        /// </summary>
        Orientation _destroySearchOrientation = Orientation.Horizontal | Orientation.Vertical;

        /// <summary>
        /// Total number of sunk positions within our possibly sunk ships
        /// </summary>
        int _sunkSpots = 0;

        /// <summary>
        /// Are we searching or destroying?
        /// </summary>
        TargettingMode _targettingMode;

        /// <summary>
        /// Total remaining positions
        /// </summary>
        int _remainingPositions;

        /// <summary>
        /// Total remaining positions (not adjacent)
        /// </summary>
        int _notAdjacentRemainingPositions;

        /// <summary>
        /// Initial search parity
        /// </summary>
        bool _parity;

        /// <summary>
        /// Whether or not to assume adjacent
        /// </summary>
        bool _assumeAdjacent = false;

        #endregion

        #region Utility Methods
        
        /// <summary>
        /// Display probabilities (DEBUG)
        /// </summary>
        public void DumpProbabilities()
        {
#if DEBUG
            Score2D[,] scoreMatrix = null;

            if (_assumeAdjacent)
            {
                scoreMatrix = _score;
            }
            else
            {
                scoreMatrix = _notAdjacentScore;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\t0\t1\t2\t3\t4\t5\t6\t7\t8\t9");
            for (int i = 0; i < AdaptivePlayer.Width; ++i)
            {
                sb.Append((char)((char)i + 'A'));
                sb.Append("\t");
                for (int j = 0; j < AdaptivePlayer.Height; ++j)
                {
                    sb.Append(scoreMatrix[i, j].Score.ToString());
                    sb.Append("\t");
                }
                sb.AppendLine();
            }
            Debug.WriteLine(sb.ToString());            

            AdaptivePlayer.Log("Position Probabilities: " + Environment.NewLine + sb.ToString());

            sb.Clear();
            sb.AppendLine("\t      0\t      1\t      2\t      3\t      4\t      5\t      6\t      7\t      8\t      9");
            for (int i = 0; i < AdaptivePlayer.Width; ++i)
            {
                sb.Append((char)((char)i + 'A'));
                sb.Append("\t");
                for (int j = 0; j < AdaptivePlayer.Height; ++j)
                {
                    sb.Append(String.Format("{0:0000.00}", Score(i, j, false) * 1000d));
                    sb.Append("\t");
                }
                sb.AppendLine();
            }
            Debug.WriteLine(sb.ToString());
            AdaptivePlayer.Log("Position Probabilities: " + Environment.NewLine + sb.ToString());

            sb.Clear();
            sb.AppendLine("\t      0\t      1\t      2\t      3\t      4\t      5\t      6\t      7\t      8\t      9");
            for (int i = 0; i < AdaptivePlayer.Width; ++i)
            {
                sb.Append((char)((char)i + 'A'));
                sb.Append("\t");
                for (int j = 0; j < AdaptivePlayer.Height; ++j)
                {
                    sb.Append(String.Format("{0:0000.00}", Score(i, j, true) * 1000d));
                    sb.Append("\t");
                }
                sb.AppendLine();
            }
            Debug.WriteLine(sb.ToString());
            AdaptivePlayer.Log("Position Probabilities: " + Environment.NewLine + sb.ToString());
#endif
        }

        /// <summary>
        /// Adjust scores based on the last shot
        /// </summary>
        void AdjustScore(Position pos)
        {
            // Clear out the score for the last shot
            _score[pos.Row, pos.Column].Clear();            

            // Adjust scores in the row
            // Search backward until we find a score of zero or an edge
            int row = pos.Row - 1;
            while ((row >= 0) && (_score[row, pos.Column].Score > 0))
            {
                --row;
            }
            int difference = pos.Row - (row + 1);

            // Apply the probability distribution over the new space
            for (var i = 0; i < difference; ++i)
            {
                _score[row + i + 1, pos.Column].ApplyVerticalScore(AdaptivePlayer.Ships, i, difference);                

                if (_score[row + i + 1, pos.Column].Score == 0)
                    AdjustScore(new Position() { Row = row + i + 1, Column = pos.Column });
            }
            
            // Search forward until we find a score of zero or an edge
            row = pos.Row + 1;
            while ((row < AdaptivePlayer.Height) && (_score[row, pos.Column].Score > 0))
            {
                ++row;
            }
            difference = row - (pos.Row + 1);

            // Apply the probability distribution over the new space
            for (var i = 0; i < difference; ++i)
            {
                _score[row - i - 1, pos.Column].ApplyVerticalScore(AdaptivePlayer.Ships, i, difference);

                if (_score[row - i - 1, pos.Column].Score == 0)
                    AdjustScore(new Position() { Row = row - i - 1, Column = pos.Column });
            }

            // Adjust scores in the column
            // Search backwards until we find a score of zero or an edge
            int col = pos.Column - 1;
            while ((col >= 0) && (_score[pos.Row, col].Score > 0))
            {
                --col;
            }
            difference = pos.Column - (col + 1);

            // Apply the probability distribution over the new space
            for (var i = 0; i < difference; ++i)
            {
                _score[pos.Row, col + i + 1].ApplyHorizontalScore(AdaptivePlayer.Ships, i, difference);

                if (_score[pos.Row, col + i + 1].Score == 0)
                    AdjustScore(new Position() { Row = pos.Row, Column = col + i + 1 });
            }

            // Search forwards until we find a score of zero or an edge
            col = pos.Column + 1;
            while ((col < AdaptivePlayer.Width) && (_score[pos.Row, col].Score > 0))
            {
                ++col;
            }

            // Apply the probability distribution over the new space
            difference = col - (pos.Column + 1);
            for (var i = 0; i < difference; ++i)
            {
                _score[pos.Row, col - i - 1].ApplyHorizontalScore(AdaptivePlayer.Ships, i, difference);

                if (_score[pos.Row, col - i - 1].Score == 0)
                    AdjustScore(new Position() { Row = pos.Row, Column = col - i - 1 });
            }

            // Determine the new total of remaining positions
            DetermineRemainingPositions();
        }

        /// <summary>
        /// Adjust scores in the not-adjacent matrix
        /// </summary>        
        void AdjustNotAdjacentScore(Position pos)
        {
            // Clear out the score for the last shot
            _notAdjacentScore[pos.Row, pos.Column].Clear();

            // Adjust scores in the row
            // Search backward until we find a score of zero or an edge
            int row = pos.Row - 1;
            while ((row >= 0) && (_notAdjacentScore[row, pos.Column].Score > 0))
            {
                --row;
            }
            int difference = pos.Row - (row + 1);

            // Apply the probability distribution over the new space
            for (var i = 0; i < difference; ++i)
            {
                _notAdjacentScore[row + i + 1, pos.Column].ApplyVerticalScore(AdaptivePlayer.Ships, i, difference);

                if (_notAdjacentScore[row + i + 1, pos.Column].Score == 0)
                    AdjustNotAdjacentScore(new Position() { Row = row + i + 1, Column = pos.Column });
            }

            // Search forward until we find a score of zero or an edge
            row = pos.Row + 1;
            while ((row < AdaptivePlayer.Height) && (_notAdjacentScore[row, pos.Column].Score > 0))
            {
                ++row;
            }
            difference = row - (pos.Row + 1);

            // Apply the probability distribution over the new space
            for (var i = 0; i < difference; ++i)
            {
                _notAdjacentScore[row - i - 1, pos.Column].ApplyVerticalScore(AdaptivePlayer.Ships, i, difference);

                if (_score[row - i - 1, pos.Column].Score == 0)
                    AdjustNotAdjacentScore(new Position() { Row = row - i - 1, Column = pos.Column });
            }

            // Adjust scores in the column
            // Search backwards until we find a score of zero or an edge
            int col = pos.Column - 1;
            while ((col >= 0) && (_notAdjacentScore[pos.Row, col].Score > 0))
            {
                --col;
            }
            difference = pos.Column - (col + 1);

            // Apply the probability distribution over the new space
            for (var i = 0; i < difference; ++i)
            {
                _notAdjacentScore[pos.Row, col + i + 1].ApplyHorizontalScore(AdaptivePlayer.Ships, i, difference);

                if (_notAdjacentScore[pos.Row, col + i + 1].Score == 0)
                    AdjustNotAdjacentScore(new Position() { Row = pos.Row, Column = col + i + 1 });
            }

            // Search forwards until we find a score of zero or an edge
            col = pos.Column + 1;
            while ((col < AdaptivePlayer.Width) && (_notAdjacentScore[pos.Row, col].Score > 0))
            {
                ++col;
            }

            // Apply the probability distribution over the new space
            difference = col - (pos.Column + 1);
            for (var i = 0; i < difference; ++i)
            {
                _notAdjacentScore[pos.Row, col - i - 1].ApplyHorizontalScore(AdaptivePlayer.Ships, i, difference);

                if (_notAdjacentScore[pos.Row, col - i - 1].Score == 0)
                    AdjustNotAdjacentScore(new Position() { Row = pos.Row, Column = col - i - 1 });
            }

            // Determine the new total of remaining positions
            DetermineRemainingPositions();
        }

        /// <summary>
        /// Determines the Neighbor score at a given position
        /// </summary>
        /// <param name="pos">Position to check</param>
        /// <returns>Neighbor score</returns>
        double NeighborScore(Position pos, bool normalize)
        {
            double neighborScore = 0;
            int neighborCount = 0;
            Score2D[,] scoreMatrix = null;

            if (_assumeAdjacent)
            {
                scoreMatrix = _score;
            }
            else
            {
                scoreMatrix = _notAdjacentScore;
            }

            // Tiebreaker is the sum of neighbor scores
            // Don't consider tiebreakers for positions marked as a hit/miss/impossible (ie. Only Score > 0)
            if ((pos.Row - 1 >= 0) && (scoreMatrix[pos.Row - 1, pos.Column].Score > 0))
            {
                neighborScore += Score(pos.Row - 1, pos.Column, false);
                neighborCount++;
            }
            if ((pos.Row + 1 < AdaptivePlayer.Height) && (scoreMatrix[pos.Row + 1, pos.Column].Score > 0))
            {
                neighborScore += Score(pos.Row + 1, pos.Column, false);
                neighborCount++;
            }
            if ((pos.Column - 1 >= 0) && (scoreMatrix[pos.Row, pos.Column - 1].Score > 0))
            {
                neighborScore += Score(pos.Row, pos.Column - 1, false);
                neighborCount++;
            }
            if ((pos.Column + 1 < AdaptivePlayer.Width) && (scoreMatrix[pos.Row, pos.Column + 1].Score > 0))
            {
                neighborScore += Score(pos.Row, pos.Column + 1, false);
                neighborCount++;
            }

            if (normalize)
            {
                if (neighborCount == 0)
                    return 0.0d;
                else
                    return neighborScore / (double)(neighborCount);
            }

            return neighborScore;
        }

        /// <summary>
        /// Find next best shots in destroy mode
        /// </summary>
        /// <returns>List of possible best shots</returns>
        List<Position> DestroyShot()
        {
            List<Position> maxProbability = new List<Position>(10);
            double maxValue = 0.0;
            Score2D[,] scoreMatrix = null;

            if (_assumeAdjacent)
            {
                scoreMatrix = _score;
            }
            else
            {
                scoreMatrix = _notAdjacentScore;
            }

            // Iterate through our active hits and shoot at the next most likely spot
            foreach (Position p in _hits)
            {
                // Check adjacent positions
                if ((_destroySearchOrientation & Orientation.Vertical) != Orientation.None)
                {
                    if ((p.Row - 1 >= 0) && (scoreMatrix[p.Row - 1, p.Column].Score > 0))
                        CheckAdjacent(p.Row - 1, p.Column, maxProbability, ref maxValue);
                    if ((p.Row + 1 < AdaptivePlayer.Height) && (scoreMatrix[p.Row + 1, p.Column].Score > 0))
                        CheckAdjacent(p.Row + 1, p.Column, maxProbability, ref maxValue);
                }

                if ((_destroySearchOrientation & Orientation.Horizontal) != Orientation.None)
                {
                    if ((p.Column - 1 >= 0) && (scoreMatrix[p.Row, p.Column - 1].Score > 0))
                        CheckAdjacent(p.Row, p.Column - 1, maxProbability, ref maxValue);
                    if ((p.Column + 1 < AdaptivePlayer.Width) && (scoreMatrix[p.Row, p.Column + 1].Score > 0))
                        CheckAdjacent(p.Row, p.Column + 1, maxProbability, ref maxValue);
                }
            }

            if (maxProbability.Count == 0)
            {
                // If we've already searched in both directions and found nothing, then our assumptions are incorrect
                if (_destroySearchOrientation == (Orientation.Vertical | Orientation.Horizontal))
                    _assumeAdjacent = true;

                // Search in both directions
                _destroySearchOrientation = Orientation.Vertical | Orientation.Horizontal;
                maxProbability = DestroyShot();
            }

            return maxProbability;
        }

        /// <summary>
        /// Checks adjacent squares for candidates
        /// </summary>
        /// <param name="row">row</param>
        /// <param name="col">column</param>
        /// <param name="candidates">list of candidates</param>
        /// <param name="maxValue">in/out max value</param>
        void CheckAdjacent(int row, int col, List<Position> candidates, ref double maxValue)
        {
            // Check the score at the position
            double score = Score(row, col, true);
            if (score > maxValue)
            {
                maxValue = score;
                candidates.Clear();
            }

            if (Math.Abs(score - maxValue) < double.Epsilon)
            {
                candidates.Add(new Position() { Row = row, Column = col });
            }
        }

        /// <summary>
        /// Choose the next best shot in Search mode
        /// </summary>
        /// <returns>The position to shoot at</returns>
        List<Position> SearchShot(bool even)
        {
            List<Position> maxProbability = new List<Position>(10);
            double maxValue = 0.0;
            Score2D[,] scoreMatrix = null;

            if (_assumeAdjacent)
            {
                scoreMatrix = _score;
            }
            else
            {
                scoreMatrix = _notAdjacentScore;
            }

            for (var i = 0; i < AdaptivePlayer.Height; ++i)
            {
                for (var j = 0; j < AdaptivePlayer.Width; ++j)
                {
                    // Check odd parity squares, then even (checkerboard pattern)
                    if ((even && ((i + j) % 2 == 0)) || (!even && ((i + j) % 2 == 1)))
                        continue;

                    // Skip squares we've already determined to be pointless to shoot at (a hit, miss, or impossible)
                    if (scoreMatrix[i, j].Score == 0)
                        continue;

                    double score = Score(i, j, true);

                    // Check the score at the position
                    if (score > maxValue)
                    {
                        maxValue = score;
                        maxProbability.Clear();
                    }

                    if (Math.Abs(score - maxValue) < double.Epsilon)
                    {
                        maxProbability.Add(new Position() { Row = i, Column = j });
                    }
                }
            }

            // If we're assuming ships aren't adjacent and we have no candidates, correct the assumption
            if (!_assumeAdjacent && maxProbability.Count <= 0)
            {
                _assumeAdjacent = true;
                maxProbability = SearchShot(even);
            }

            // Didn't find any good candidates in parity, try with other
            if (maxProbability.Count <= 0)
                maxProbability = SearchShot(!even);

            return maxProbability;
        }

        /// <summary>
        /// Adjust position possibilies based on the fact that a ship was sunk
        /// </summary>
        /// <param name="size">size of ship</param>
        void Sink(Ship ship)
        {
            // Remove this ship type from ship position consideration
            for (var i = 0; i < AdaptivePlayer.Width; ++i)
            {
                for (var j = 0; j < AdaptivePlayer.Width; ++j)
                {
                    if (_score[i, j].Score > 0)
                        _score[i, j].Sink(ship.Size);
                }
            }

            _sunkSpots += ship.Size;     

            // We sunk a ship, go back to search mode if we don't have any pending hit boats
            if (_sunkSpots == _hits.Count)
            {
                _sunkSpots = 0;

                List<Position> adjacent = new List<Position>();

                // We can now adjust scores for these hits
                foreach (Position p in _hits)
                {
                    AdjustScore(p);
                    if (!_assumeAdjacent)
                    {
                        AdjustNotAdjacentScore(p);
                        adjacent.AddRange(AdjacentPositions(p));
                    }
                }
                
                if (!_assumeAdjacent)
                {
                    adjacent = adjacent.Except(_hits).Distinct().ToList();
                    foreach (Position p in adjacent)
                    {
                        AdjustNotAdjacentScore(p);
                    }
                }

                _hits.Clear();
                _targettingMode = TargettingMode.Search;
            }
            else
            {
                // We found two ships that are adjacent
                _assumeAdjacent = true;

                // Let's see if we can remove some hits from further consideration                
                List<Ship> possibleLayouts = new List<Ship>();

                if ((_destroySearchOrientation & Orientation.Horizontal) == Orientation.Horizontal)
                {
                    Ship layout = ship.Clone() as Ship;
                    layout.Orientation = Orientation.Horizontal;

                    // Let's see if we can find the end points of the ship
                    var possibilities = _hits.Where(p => p.Row == _lastShot.Row && Math.Abs(p.Column - _lastShot.Column) < ship.Size);
                    
                    if (possibilities.Count() == ship.Size)
                    {
                        layout.Position = possibilities.Min();
                        if (!layout.Positions.Except(_hits).Any())
                        {
                            // If all positions in the layout are contained in hits
                            possibleLayouts.Add(layout);
                        }
                    }
                }

                if ((_destroySearchOrientation & Orientation.Vertical) == Orientation.Vertical)
                {
                    Ship layout = ship.Clone() as Ship;
                    layout.Orientation = Orientation.Vertical;

                    // Let's see if we can find the end points of the ship
                    var possibilities = _hits.Where(p => p.Column == _lastShot.Column && Math.Abs(p.Row - _lastShot.Row) < ship.Size);

                    if (possibilities.Count() == ship.Size)
                    {
                        layout.Position = possibilities.Min();
                        if (!layout.Positions.Except(_hits).Any())
                        {
                            // If all positions in the layout are contained in hits
                            possibleLayouts.Add(layout);
                        }
                    }
                }

                if (possibleLayouts.Count == 1)
                {
                    // Remove these positions from consideration
                    _hits = _hits.Except(possibleLayouts[0].Positions).ToList();
                    _sunkSpots -= ship.Size;

                    foreach (Position p in possibleLayouts[0].Positions)
                        AdjustScore(p);
                }
            }

            // Reset our destroy orientation pattern
            _destroySearchOrientation = Orientation.Horizontal | Orientation.Vertical;       
        }

        /// <summary>
        /// Determines the historical hit probability
        /// </summary>
        /// <param name="p">position</param>
        /// <returns>Probability of a hit (smoothed with laplacian smoothing)</returns>
        double HistoricalHitProbability(int row, int col)
        {
            return (double)(AdaptivePlayer.Data.OutgoingHits[row][col] + _laplaceSmoothHits) / (double)(AdaptivePlayer.Data.OutgoingHits[row][col] + AdaptivePlayer.Data.OutgoingMisses[row][col] + _laplaceSmoothFactor);
        }

        /// <summary>
        /// Determines total probability at position p
        /// </summary>
        /// <param name="p">position</param>
        /// <returns>shot score at position p</returns>
        double Score(int row, int col, bool includeNeighbor)
        {
            Score2D[,] scoreMatrix = null;
            double remainingPositions = 1.0d;

            if (_assumeAdjacent)
            {
                scoreMatrix = _score;
                remainingPositions = (double)_remainingPositions;
            }
            else
            {
                scoreMatrix = _notAdjacentScore;
                remainingPositions = (double)_notAdjacentRemainingPositions;
            }

            return ((double)((double)scoreMatrix[row, col].Score) / remainingPositions) * HistoricalHitProbability(row, col) + (includeNeighbor ? NeighborScore(new Position(row, col), true) : 0);
        }

        /// <summary>
        /// Adjust the total remaining positions
        /// </summary>
        void DetermineRemainingPositions()
        {
            _remainingPositions = 0;
            _notAdjacentRemainingPositions = 0;
            for (var i = 0; i < AdaptivePlayer.Height; ++i)
            {
                for (var j = 0; j < AdaptivePlayer.Width; ++j)
                {
                    _remainingPositions += _score[i, j].Score;
                    _notAdjacentRemainingPositions += _notAdjacentScore[i, j].Score;
                }
            }
        }

        /// <summary>
        /// Returns all positions adjacent to a position
        /// </summary>
        /// <param name="p">Position</param>
        /// <returns>Enumerable of all adjacent positions</returns>
        IEnumerable<Position> AdjacentPositions(Position p)
        {
            if (p.Row > 0)
                yield return new Position(p.Row - 1, p.Column);
            if (p.Row < Player.Height - 1)
                yield return new Position(p.Row + 1, p.Column);
            if (p.Column > 0)
                yield return new Position(p.Row, p.Column - 1);
            if (p.Column < Player.Width - 1)
                yield return new Position(p.Row, p.Column + 1);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Owning player
        /// </summary>
        public AdaptiveBattleshipPlayer AdaptivePlayer { get; private set; }

        #endregion

        #region IBattleshipOffense Implmentation

        /// <summary>
        /// Owning player
        /// </summary>
        public BattleshipPlayer Player { get { return AdaptivePlayer; } }
        
        /// <summary>
        /// Initializes a battleship offense
        /// </summary>
        /// <param name="player">owning player</param>  
        public void Initialize(BattleshipPlayer player)
        {            
            AdaptivePlayer = player as AdaptiveBattleshipPlayer;
            _laplaceSmoothHits = (int)((double)_laplaceSmoothFactor * (double)AdaptivePlayer.Ships.Sum(x => x.Size) / (double)(AdaptivePlayer.Width * AdaptivePlayer.Height));
            _score = BattleshipStatisticalUtility.ShipPossibilityMatrix(AdaptivePlayer.Width, AdaptivePlayer.Height, AdaptivePlayer.Ships);
            _notAdjacentScore = BattleshipStatisticalUtility.ShipPossibilityMatrix(AdaptivePlayer.Width, AdaptivePlayer.Height, AdaptivePlayer.Ships);

            DetermineRemainingPositions();

            _parity = BattleshipGame.Random.NextDouble() < 0.5;

            // We start out assuming they don't allow adjacent ships, if we find out that they do in any play through, assume that the do
            if (AdaptivePlayer.Data.AllowsAdjacent > 0)
                _assumeAdjacent = true;
            else
                _assumeAdjacent = false;            
        }

        /// <summary>
        /// Fires at a position
        /// </summary>
        /// <returns>Position to fire at</returns>
        public Position Fire()
        {            
#if DEBUG
            DumpProbabilities();
#endif
            // Find a set of positions that are the most probable            
            List<Position> maxProbability = null;

            // If we aren't locked on to a sub, use the search algorithm - otherwise use the sink algorithm
            if (_targettingMode == TargettingMode.Search)
            {
                maxProbability = SearchShot(_parity);
            }
            else
            {
                maxProbability = DestroyShot();
            }

            // We should have at least one square
            Debug.Assert(maxProbability.Count > 0);

            // Only one shot returned
            if (maxProbability.Count == 1)
            {             
                _lastShot = maxProbability[0];            
            }
            // If we have more than one candidate
            else if (maxProbability.Count > 1)
            {
                // Determine tie breakers
                List<Position> tieBreakers = new List<Position>(5);

                double maxValue = 0;
                foreach (Position p in maxProbability)
                {
                    double tieBreaker = NeighborScore(p, false);
                    if (tieBreaker > maxValue)
                    {
                        maxValue = tieBreaker;
                    }

                    if (tieBreaker == maxValue)
                    {
                        tieBreakers.Add(p);
                    }
                }

                Debug.Assert(tieBreakers.Count > 0);                
                
                // Pick among the best tie breakers at random
                if (tieBreakers.Count > 0)
                {
                    int index = BattleshipGame.Random.Next(tieBreakers.Count);


                    _lastShot = tieBreakers[index];
                }                            
            }
            else
            {
                // We should never get here, but just in case, we'll choose a random location
                bool found = false;
                int limit = 0;

                // Keep looking until we've checked every square or found a place we haven't shot
                while (!found && limit < AdaptivePlayer.Width * AdaptivePlayer.Height)
                {
                    // Choose a random location
                    int row = BattleshipGame.Random.Next(AdaptivePlayer.Height);
                    int col = BattleshipGame.Random.Next(AdaptivePlayer.Width);

                    // If we haven't shot at the location yet, shoot at it
                    if (_score[row, col].Score != 0)
                    {
                        _lastShot = new Position(row, col);
                        found = true;
                    }

                    limit++;
                }
            }

            return _lastShot;
        }

        /// <summary>
        /// Register last fire as a miss
        /// </summary>
        public void Miss()
        {                       
            // Adjust the score of remaining squares
            AdjustScore(_lastShot);
            if (!_assumeAdjacent)
                AdjustNotAdjacentScore(_lastShot);
        }
        
        /// <summary>
        /// Register last fire as a hit
        /// </summary>
        public void Hit()
        {            
            // List of hits in the current sinking process
            _hits.Add(_lastShot);

            // Full list of hits
            _allHits.Add(_lastShot);

            // Clear out the score for the last shot
            _score[_lastShot.Row, _lastShot.Column].Clear();
            _notAdjacentScore[_lastShot.Row, _lastShot.Column].Clear();

            // Determine the new total of remaining positions
            DetermineRemainingPositions();

            // If we're not already in destroy mode, set it and allow the algorithm to choose best choice among horizontal and vertical directions
            if (_targettingMode != TargettingMode.Destroy)
            {
                _destroySearchOrientation = Orientation.Horizontal | Orientation.Vertical;
                _targettingMode = TargettingMode.Destroy;
            }
            else
            {
                // We're already in destroy mode, try to determine the orientation of the ship
                if (_hits.Count > 1)
                {
                    if (_hits[_hits.Count - 1].Row == _hits[_hits.Count - 2].Row)
                        _destroySearchOrientation = Orientation.Horizontal;
                    else if (_hits[_hits.Count - 1].Column == _hits[_hits.Count - 2].Column)
                        _destroySearchOrientation = Orientation.Vertical;
                }
            }
        }

        /// <summary>
        /// Notify that a ship has sunk
        /// </summary>
        /// <param name="s">Code representing the ship</param>
        public void Sink(string s)
        {
            Ship ship = AdaptivePlayer.Ships.Where(x => String.Compare(x.Code, s) == 0).FirstOrDefault();
            if (ship != null)
            {
                Sink(ship);
                AdaptivePlayer.Ships.Remove(ship);
            }            
        }

        /// <summary>
        /// Enemy positions sent to us after game ends
        /// </summary>
        /// <param name="_ships">Positions of enemy ships</param>
        public void RegisterEnemyPositions(List<Ship> ships)
        {
            // Since we get full knowledge of whatever ships the enemy has placed, use that to adjust our gameplay
            // We won't need to track this as we play

            // Mark every square as a miss
            for (var i = 0; i < AdaptivePlayer.Height; ++i)
                for (var j = 0; j < AdaptivePlayer.Width; ++j)
                    AdaptivePlayer.Data.OutgoingMisses[i][j]++;

            // If we lost, mark all positions with ships as hits and take away the miss
            foreach (Ship ship in ships)
            {
                foreach (Position p in ship.Positions)
                {
                    AdaptivePlayer.Data.OutgoingMisses[p.Row][p.Column]--;
                    AdaptivePlayer.Data.OutgoingHits[p.Row][p.Column]++;
                }
            }

            // Check to see if any ships are adjacent
            bool adjacent = false;
            for (var i = 0; i < ships.Count && !adjacent; ++i)
            {
                for (var j = i + 1; j < ships.Count && !adjacent; ++j)
                {
                    if (ships[i].Adjacent(ships[j]))
                        adjacent = true;
                }
            }

            if (adjacent)
                AdaptivePlayer.Data.AllowsAdjacent++;
        }

        /// <summary>
        /// Cleans up an offense
        /// </summary>
        /// <param name="state"></param>
        public void Cleanup(GameEndState state)
        {

        }

        #endregion
    }
}
