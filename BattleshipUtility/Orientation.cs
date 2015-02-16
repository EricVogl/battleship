using System;

namespace BattleshipUtility
{
    /// <summary>
    /// Orientation of a ship
    /// </summary>
    [Flags]
    public enum Orientation
    {
        /// <summary>
        /// None/Unknown
        /// </summary>
        None = 0,

        /// <summary>
        /// Horizontal
        /// </summary>
        Horizontal = 1,

        /// <summary>
        /// Vertical
        /// </summary>
        Vertical = 1 << 1
    }
}
