using System;
using System.Collections.Generic;
using BattleshipUtility;

namespace RandomBattleship
{
    class Program
    {
        static void Main(string[] args)
        {
            string opponent;
            if (args == null || args.Length < 1)
                // No opponent name supplied, create one
                opponent = Guid.NewGuid().ToString();
            else
                opponent = args[0];

            RandomBattleshipPlayer player = new RandomBattleshipPlayer();
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
    }
}
