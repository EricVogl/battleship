using BattleshipUtility;
using System.IO.IsolatedStorage;
using System.IO;
using System.Xml.Serialization;
using System;

namespace EricVoglBattleship
{
    /// <summary>
    /// Battleship player who plays the odds and attempts to learn from opponent
    /// </summary>
    public sealed class AdaptiveBattleshipPlayer : BattleshipPlayer
    {
        #region Constructor

        /// <summary>
        /// Creates a new random battleship player
        /// </summary>
        public AdaptiveBattleshipPlayer()
        {
            Offense = new AdaptiveBattleshipOffense();
            Defense = new AdaptiveBattleshipDefense();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Data about opponent
        /// </summary>
        public BattleshipData Data { get; private set; }

        /// <summary>
        /// Filename for opponent data
        /// </summary>
        public string FileName { get; private set; }

        #endregion

        #region Battleship Player Implementation

        /// <summary>
        /// Loads saved data
        /// </summary>
        public override void Load()
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
            {
                FileName = Opponent + "_" + Width.ToString() + "_" + Height.ToString();
                if (!isoStore.FileExists(FileName))
                {
                    // Prime the shots matrix with the number of different ship positions that are possible at a given location
                    Data = new BattleshipData();
                    Data.IncomingShots = new int[Width][];
                    Data.OutgoingHits = new int[Width][];
                    Data.OutgoingMisses = new int[Width][];
                    for (int i = 0; i < Data.IncomingShots.Length; ++i)
                    {
                        Data.IncomingShots[i] = new int[Height];
                        Data.OutgoingHits[i] = new int[Height];
                        Data.OutgoingMisses[i] = new int[Height];
                    }
                    Data.MinTurns = int.MaxValue;
                }
                else
                {
                    using (var stream = isoStore.OpenFile(FileName, FileMode.Open))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(BattleshipData));
                        Data = serializer.Deserialize(stream) as BattleshipData;
                    }
                }
            }
        }

        /// <summary>
        /// Saves gathered data
        /// </summary>
        public override void Save(GameEndState state)
        {
            if (state == GameEndState.Win)
                ++Data.Wins;
            else if (state == GameEndState.Loss)
                ++Data.Losses;
            else if (state == GameEndState.Tie)
                ++Data.Ties;

            if (Turn < Data.MinTurns)
                Data.MinTurns = Turn;
            if (Turn > Data.MaxTurns)
                Data.MaxTurns = Turn;

            Data.AverageTurns = (Data.AverageTurns * (float)(Data.Wins + Data.Losses + Data.Ties - 1) + (float)Turn) / (float)(Data.Wins + Data.Losses + Data.Ties);

            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
            {
                using (var stream = isoStore.CreateFile(FileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(BattleshipData));
                    serializer.Serialize(stream, Data);
                }

#if DEBUG
                using (var stream = isoStore.CreateFile(FileName + "_Log_" + DateTime.Now.ToString("yyyyMMddhhmmss")))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(_log.ToString());
                    }
                }
                _log.Clear();
#endif
            }     
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
