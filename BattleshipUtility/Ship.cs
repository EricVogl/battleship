using System;
using System.Collections.Generic;

namespace BattleshipUtility
{
    /// <summary>
    /// Describes a ship
    /// </summary>
    public sealed class Ship : ICloneable
    {
        #region Fields

        /// <summary>
        /// Top-Left Position of the ship on the game board
        /// </summary>
        public Position Position;

        #endregion

        #region Properties

        /// <summary>
        /// Code describing the ship
        /// </summary>
        public string Code { get; set; }        
        
        /// <summary>
        /// Size of the ship
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Orientation of the ship
        /// </summary>
        public Orientation Orientation { get; set; }

        /// <summary>
        /// List of positions of a ship
        /// </summary>
        public IEnumerable<Position> Positions
        {
            get
            {
                for (var i = 0; i < Size; ++i)
                {
                    if (Orientation == Orientation.Vertical)
                    {
                        yield return new Position(Position.Row + i, Position.Column);
                    }
                    else
                    {
                        yield return new Position(Position.Row, Position.Column + i);
                    }
                }
            }
        }
        
        #endregion

        #region Methods        

        /// <summary>
        /// Determines if a ship occupies a position
        /// </summary>
        /// <param name="p">Position to check</param>
        /// <returns>True if any part of a ship occupies the position</returns>
        public bool At(Position p)
        {
            if (Orientation == Orientation.Horizontal)
            {
                return (Position.Row == p.Row) && (Position.Column <= p.Column) && (Position.Column + Size > p.Column);
            }
            else
            {
                return (Position.Column == p.Column) && (Position.Row <= p.Row) && (Position.Row + Size > p.Row);
            }
        }

        /// <summary>
        /// Creates a 1-space border around a ship and checks if a position is within this border
        /// </summary>
        /// <param name="p">Position to check</param>
        /// <returns>True if a shot is adjacent to a ship</returns>
        public bool AtOrAdjacent(Position p)
        {
            if (Orientation == Orientation.Horizontal)
            {
                return ((Math.Abs(Position.Row - p.Row) < 2) && (Position.Column <= p.Column) && (Position.Column + Size > p.Column)) 
                    || ((Position.Row == p.Row) && (Position.Column <= p.Column + 1) && (Position.Column + Size >= p.Column));
            }
            else
            {
                return ((Math.Abs(Position.Column - p.Column) < 2) && (Position.Row <= p.Row) && (Position.Row + Size > p.Row)) 
                    || ((Position.Column == p.Column) && (Position.Row <= p.Row + 1) && (Position.Row + Size >= p.Row));
            }
        }

        /// <summary>
        /// Determines if a point is adjacent to a ship
        /// </summary>
        /// <param name="p">position</param>
        /// <returns>true if adjacent</returns>
        public bool Adjacent(Position p)
        {
            if (Orientation == Orientation.Horizontal)
            {
                return ((Math.Abs(Position.Row - p.Row) == 1) && (Position.Column <= p.Column) && (Position.Column + Size > p.Column))
                    || ((Position.Row == p.Row) && (Position.Column == p.Column + 1) && (Position.Column + Size == p.Column));
            }
            else
            {
                return ((Math.Abs(Position.Column - p.Column) == 1) && (Position.Row <= p.Row) && (Position.Row + Size > p.Row))
                    || ((Position.Column == p.Column) && (Position.Row == p.Row + 1) && (Position.Row + Size == p.Row));
            }
        }

        /// <summary>
        /// Whether or not a ship intersects another ship
        /// </summary>
        /// <param name="ship"></param>
        /// <returns></returns>
        public bool Intersects(Ship ship)
        {
            foreach (Position p in ship.Positions)
            {
                if (At(p)) return true;
            }
            return false;
        }

        /// <summary>
        /// Whether or not a ship intersects or is next to another ship
        /// </summary>
        /// <param name="ship"></param>
        /// <returns></returns>
        public bool IntersectsOrAdjacent(Ship ship)
        {
            foreach (Position p in ship.Positions)
            {
                if (AtOrAdjacent(p)) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if a ship is adjacent to this ship
        /// </summary>
        /// <param name="ship"></param>
        /// <returns></returns>
        public bool Adjacent(Ship ship)
        {
            foreach (Position p in ship.Positions)
            {
                if (Adjacent(p)) return true;
            }
            return false;
        }

        /// <summary>
        /// Randomly places a ship on a grid
        /// </summary>
        /// <param name="width">width of grid</param>
        /// <param name="height">height of grid</param>
        public void Place(int width, int height)
        {
            // Randomly choose a position and orientation
            int row = BattleshipGame.Random.Next(height);
            int column = BattleshipGame.Random.Next(width);
            Orientation = (BattleshipGame.Random.NextDouble() < 0.5d) ? Orientation.Horizontal : Orientation.Vertical;

            // Make sure the ship stays on the board
            if ((Orientation == Orientation.Horizontal) && ((column + Size) >= width))
            {
                column = column - Size + 1;
            } 
            else if ((Orientation == Orientation.Vertical) && ((row + Size) >= height))
            {
                row = row - Size + 1;
            }

            Position.Row = row;
            Position.Column = column;
        }

        /// <summary>
        /// Describes a ship
        /// </summary>
        /// <returns>String description of a ship</returns>
        public override string ToString()
        {
            Position endPosition = new Position();
            if (Orientation == Orientation.Horizontal)
            {
                endPosition.Row = Position.Row;
                endPosition.Column = Position.Column + Size - 1;
            }
            else 
            {
                endPosition.Row = Position.Row + Size - 1;
                endPosition.Column = Position.Column;
            }

            return String.Format("{0} {1} {2}", Code, Position.ToString(), endPosition.ToString());
        }

        /// <summary>
        /// List of positions adjacent to a ship
        /// </summary>
        public IEnumerable<Position> AdjacentPositions(int width, int height)
        {
            if (Orientation == Orientation.Vertical)
            {
                if (Position.Row > 0)
                    yield return new Position(Position.Row - 1, Position.Column);
                if (Position.Row + Size < height)
                    yield return new Position(Position.Row + 1, Position.Column);
                for (var i = 0; i < Size; ++i)
                {
                    if (Position.Column > 0)
                        yield return new Position(Position.Row, Position.Column - 1);
                    if (Position.Column + Size < width)
                        yield return new Position(Position.Row, Position.Column + 1);
                }
            }
            else
            {                
                if (Position.Column > 0)
                    yield return new Position(Position.Row, Position.Column - 1);
                if (Position.Column + Size < width)
                    yield return new Position(Position.Row, Position.Column + 1);
                for (var i = 0; i < Size; ++i)
                {
                    if (Position.Row > 0)
                        yield return new Position(Position.Row - 1, Position.Column);
                    if (Position.Row + Size < height)
                        yield return new Position(Position.Row + 1, Position.Column);
                }
            }
        }

        #endregion

        #region Static Fields

        static char[] delimiters = new char[] { ' ' };

        #endregion

        #region Static Methods

        public static Ship Parse(string s)
        {
            string[] parts = s.Split(delimiters);
            if (parts.Length != 3)
                throw new ArgumentException("s");

            Position p1 = Position.Parse(parts[1]);
            Position p2 = Position.Parse(parts[2]);

            if (p2 < p1)
            {
                Position swap = p1;
                p1 = p2;
                p2 = swap;
            }
            Ship ship = new Ship() { Position = p1, Size = p1 - p2 + 1, Orientation = (p1.Row == p2.Row) ? Orientation.Horizontal : Orientation.Vertical, Code = parts[0] };
            return ship;
        }

        #endregion

        #region ICloneable
        
        public object Clone()
        {
            return new Ship() { Code = this.Code, Orientation = this.Orientation, Size = this.Size, Position = this.Position };
        }

        #endregion
    }
}
