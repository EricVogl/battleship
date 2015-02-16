using System;
using System.Diagnostics;
using System.Text;

namespace BattleshipUtility
{
    /// <summary>
    /// Describes a board position
    /// </summary>
    public struct Position : IEquatable<Position>, IComparable<Position>
    {
        #region Static Fields

        public static Position Empty { get; private set; }

        #endregion

        #region Static Constructor

        static Position()
        {
            Empty = new Position(-1, -1);
        }

        #endregion

        #region Fields        

        /// <summary>
        /// Row on board
        /// </summary>
        public int Row;

        /// <summary>
        /// Column on board
        /// </summary>
        public int Column;

        #endregion

        #region Constructors        

        /// <summary>
        /// Creates a new position
        /// </summary>
        /// <param name="row">Row on board</param>
        /// <param name="column">Column on board</param>
        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Displays a new position
        /// </summary>
        /// <returns>String representation of a position</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(2);
            sb.Append((char)((char)Row + 'A'));
            sb.Append((char)((char)Column + '0'));
            return sb.ToString();
        }

        /// <summary>
        /// Determines if one position is equal to another
        /// </summary>
        /// <param name="other">the other position</param>
        /// <returns>true if equal</returns>
        public bool Equals(Position other)
        {
            if ((other.Row == Row) && (other.Column == Column))
                return true;
            return false;
        }

        /// <summary>
        /// Determines if one position is equal to another
        /// </summary>
        /// <param name="obj">the other position</param>
        /// <returns>true if equal</returns>
        public override bool Equals(object obj)
        {
            return Equals((Position)obj);
        }        

        /// <summary>
        /// Compares a position to another position
        /// </summary>
        /// <param name="other"></param>
        /// <returns>less than zero if this is less than other, greater if this is greater than other, zero if equal</returns>
        public int CompareTo(Position other)
        {
            if (Equals(other))
                return 0;

            if ((Row < other.Row) || ((Row == other.Row) && (Column < other.Column)))
                return -1;
            else
                return 1;
        }

        /// <summary>
        /// Generates a hashcode for a position
        /// </summary>
        /// <returns>hashcode</returns>
        public override int GetHashCode()
        {
            return Row ^ Column;
        }

        #endregion

        #region Static Methods        

        /// <summary>
        /// Parses a position
        /// </summary>
        /// <param name="s">string representation of a position</param>
        /// <returns></returns>
        public static Position Parse(string s)
        {
            Debug.Assert(!String.IsNullOrEmpty(s) && s.Length == 2);
            Position p = new Position();

            p.Row = s[0] - 'A';
            p.Column = s[1] - '0';

            return p;
        }

        #endregion        
    
        #region Operator Overloads
        

        public static bool operator <(Position p1, Position p2)
        {
            return p1.CompareTo(p2) < 0;
        }

        public static bool operator <=(Position p1, Position p2)
        {
            return p1.CompareTo(p2) <= 0;
        }

        public static bool operator >(Position p1, Position p2)
        {
            return p1.CompareTo(p2) > 0;
        }

        public static bool operator >=(Position p1, Position p2)
        {
            return p1.CompareTo(p2) >= 0;
        }

        public static bool operator ==(Position a, Position b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Position a, Position b)
        {
            return !a.Equals(b);
        }

        /// <summary>
        /// Used to calculate size
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int operator -(Position a, Position b)
        {
            if (a.Row == b.Row)
                return Math.Abs(a.Column - b.Column);
            
            if (a.Column == b.Column)
                return Math.Abs(a.Row - b.Row);

            throw new InvalidOperationException("Positions are not collinear");
        }

        #endregion
    }
}
