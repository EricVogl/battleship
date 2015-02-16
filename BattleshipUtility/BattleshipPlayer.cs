using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using System.Xml.Serialization;

namespace BattleshipUtility
{
    /// <summary>
    /// Describes a battleship player
    /// </summary>
    public abstract class BattleshipPlayer
    {
        #region Debug Fields

#if DEBUG
        protected StringBuilder _log = new StringBuilder();
#endif
        #endregion

        #region Properties

        /// <summary>
        /// Offense in use
        /// </summary>
        public abstract IBattleshipOffense Offense { get; protected set; }

        /// <summary>
        /// Defense in use
        /// </summary>
        public abstract IBattleshipDefense Defense { get; protected set; }       

        /// <summary>
        /// Opponent name
        /// </summary>
        public string Opponent { get; private set; }

        /// <summary>
        /// Width of board
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Height of board
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Current turn
        /// </summary>
        public int Turn { get; private set; }

        /// <summary>
        /// Ships in use for this game
        /// </summary>
        public List<Ship> Ships { get; private set; }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Loads any saved data about opponents
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Saves any gathered data about opponents
        /// </summary>
        public abstract void Save(GameEndState state);

        #endregion

        #region Methods

        /// <summary>
        /// Creates a player
        /// </summary>
        /// <param name="width">width of board</param>
        /// <param name="height">height of board</param>
        /// <param name="ships">ships in use this game</param>
        /// <param name="opponent">opponent name</param>
        /// <param name="offense">offense to use</param>
        /// <param name="defense">defense to use</param>
        public void Initialize(int width, int height, List<Ship> ships, string opponent)
        {
            BattleshipGame.Seed(opponent);

            Opponent = opponent;            
            Width = width;
            Height = height;
            Turn = 1;
            Ships = ships;

            Load();

            Offense.Initialize(this);
            Defense.Initialize(this);
        }

        /// <summary>
        /// Cleans up the game
        /// </summary>
        /// <param name="state">win/lose/tie</param>
        public void Cleanup(GameEndState state)
        {
            Offense.Cleanup(state);
            Defense.Cleanup(state);

            Save(state);
        }
        
        /// <summary>
        /// Advances to next turn
        /// </summary>
        public void NextTurn()
        {
            Turn = Turn + 1;
        }
        
#if DEBUG
        /// <summary>
        /// Stores a game log
        /// </summary>
        /// <param name="s">string to log</param>
        /// <remarks>You can choose to save or process this in your player implementation</remarks>
        public void Log(string s)
        {
            _log.AppendLine(s);
        }
#endif
        #endregion
    }
}
