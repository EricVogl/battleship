using BattleshipUtility;

namespace RandomBattleship
{
    /// <summary>
    /// Sample class to demonstrate usage of the battleship utility library
    /// </summary>
    public sealed class RandomBattleshipPlayer : BattleshipPlayer
    {
        #region Constructor

        /// <summary>
        /// Creates a new random battleship player
        /// </summary>
        public RandomBattleshipPlayer()
        {
            Offense = new RandomBattleshipOffense();
            Defense = new RandomBattleshipDefense();
        }

        #endregion

        #region Battleship Player Implementation

        /// <summary>
        /// Loads saved data
        /// </summary>
        public override void Load()
        {
            // This is where we would load any data we saved about an opponent            
            // Random Player, however, doesn't adapt and doesn't need to load anything
        }

        /// <summary>
        /// Saves gathered data
        /// </summary>
        public override void Save(GameEndState state)
        {
            // This is where we would save any dataabout an opponent
            // Random Player, however, doesn't adapt and doesn't need to save anything
        }

        /// <summary>
        /// Offensive strategy for random player
        /// </summary>
        public override IBattleshipOffense Offense { get; protected set; }        

        /// <summary>
        /// Defensive strategy for random player
        /// </summary>
        public override IBattleshipDefense Defense { get; protected set; }

        #endregion
    }
}
