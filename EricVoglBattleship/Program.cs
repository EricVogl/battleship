using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml.Serialization;
using BattleshipUtility;

namespace EricVoglBattleship
{
    /// <summary>
    /// Main program
    /// </summary>
    internal sealed class Program
    {       
        #region Methods        

        /// <summary>
        /// Program entry method
        /// </summary>
        /// <param name="args">command line arguments - unused</param>
        static void Main(string[] args)
        {
            string opponent;
            if (args == null || args.Length < 1)
                // No opponent name supplied, create one
                opponent = Guid.NewGuid().ToString();
            else
                opponent = args[0];

            AdaptiveBattleshipPlayer player = new AdaptiveBattleshipPlayer();
            BattleshipGame game = new BattleshipGame();
            List<Ship> ships = new List<Ship>()
            {
                new Ship() { Code = "D", Size = 2 },
                new Ship() { Code = "S", Size = 3 },
                new Ship() { Code = "S", Size = 3 },
                new Ship() { Code = "C", Size = 4 },
                new Ship() { Code = "B", Size = 5 }
            };

            game.GameLoop(opponent, 10, 10, ships, player);
        }

        #endregion
    }
}
