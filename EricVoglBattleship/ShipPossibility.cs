namespace EricVoglBattleship
{
    /// <summary>
    /// Utility class to track possible positions
    /// </summary>
    public class ShipPossibility
    {
        /// <summary>
        /// Size of ship
        /// </summary>
        public int Size;

        /// <summary>
        /// Possible positions in one dimension
        /// </summary>
        public int Possibilities;

        /// <summary>
        /// Whether or not this ship was processed this pass
        /// </summary>
        public bool Processed;
    }
}
