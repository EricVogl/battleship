using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace BattleshipUtility
{
    /// <summary>
    /// An instance of a battleship game
    /// </summary>
    public class BattleshipGame
    {
        #region Static Fields
        
        /// <summary>
        /// Incoming command delimiter
        /// </summary>
        static char[] delimiters = { ' ' };
        
        /// <summary>
        /// Random number generator
        /// </summary>
        static Random _rand;
               
        #endregion

        #region Static Properties

        public static Random Random { get { return _rand; } }

        #endregion

        #region Static Methods
        
        public static void Seed(string opponent)
        {
            _rand = new Random(Environment.TickCount + opponent.GetHashCode());
        }

        #endregion

        #region Instance Fields

        /// <summary>
        /// Command dictionary
        /// </summary>
        Dictionary<string, Func<string, CommandResult>> _commands;

        // Board width
        int _width;

        // Board height
        int _height;

        // Total number of ships
        int _shipCount;

        // Ships in play
        List<Ship> _ships;

        /// <summary>
        /// Battleship player
        /// </summary>
        BattleshipPlayer _player;

        #endregion

        #region Instance Methods

        /// <summary>
        /// Main battleship game loop
        /// </summary>
        /// <param name="opponent">Name of opponent</param>
        /// <param name="width">Width of board</param>
        /// <param name="height">Height of board</param>
        /// <param name="ships">Ships in play</param>
        /// <param name="offense">Offensive strategy</param>
        /// <param name="defense">Defensive strategy</param>
        public void GameLoop(string opponent, int width, int height, List<Ship> ships, BattleshipPlayer player)
        {
            #region Argument Validation            

            if (width < 1)
                throw new ArgumentException("width");

            if (height < 1)
                throw new ArgumentException("height");

            if (ships == null)
                throw new ArgumentNullException("ships");

            if (ships.Count == 0)
                throw new ArgumentException("ships");

            if (player == null)
                throw new ArgumentNullException("player");
            
            #endregion
            
            _width = width;
            _height = height;
            _ships = ships;
            _shipCount = ships.Count;
            _player = player;

            // Initialize the command dictionary
            _commands =  new Dictionary<string, Func<string, CommandResult>>()
            {
                { "accept", Accept },
                { "reject", Reject },
                { "fire", Fire },
                { "hit", Hit },
                { "miss", Miss },
                { "incoming", Incoming },
                { "sink", Sink },
                { "win", Win },
                { "loss", Loss },
                { "tie", Tie },            
                { "exit", Exit }
            };
            
            try
            {
                // Initialize the player
                CommandResult lastResult = CommandResult.Continue;

                _player.Initialize(_width, _height, _ships, opponent);

                // Write out initial positions of the ships
                List<Ship> placedShips = _player.Defense.PlaceShips();
                foreach (Ship ship in placedShips)
                {
                    Console.WriteLine(ship.ToString());
                    Console.Out.Flush();
#if DEBUG
                    _player.Log(">> " + ship.ToString());
#endif
                }

                // Main game loop
                do
                {
                    // Get incoming command
                    string command = Console.ReadLine();
#if DEBUG
                    _player.Log("<< " + command);
#endif

                    if (String.IsNullOrEmpty(command))
                    {
                        Debug.WriteLine("Unexpected input: No data received.");
                        break;
                    }

                    // Split the command up
                    string[] parsedCommand = command.Split(delimiters);

                    if (parsedCommand != null && parsedCommand.Length > 0)
                    {
                        // See if an argument was sent with the command
                        string commandArgument = null;
                        if (parsedCommand.Length > 1)
                        {
                            commandArgument = parsedCommand[1];
                        }

                        // Determine what command was sent
                        Func<string, CommandResult> commandFunction;
                        if (_commands.TryGetValue(parsedCommand[0].ToLower(), out commandFunction))
                        {
                            lastResult = commandFunction(commandArgument);
                        }
                        else
                        {
                            Debug.WriteLine("Invalid command: " + command);
                        }
                    }
                    else
                    {
                        Debug.Write("Failed to parse command, attempting to continue.");
                    }
                } while (lastResult == CommandResult.Continue);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error occurred.  Exception: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
                player.Log("Unexpected error occurred.  Exception: " + ex.Message);
                player.Log("Stack Trace: " + ex.StackTrace);
                player.Cleanup(GameEndState.Loss);
                return;
            }

            Debug.WriteLine("Exiting...");
        }
    
        /// <summary>
        /// Handles the 'accept' command
        /// </summary>
        /// <param name="s">Unused</param>
        /// <returns>Continue</returns>
        CommandResult Accept(string s)
        {
            Debug.WriteLine("Ship layout accepted.");
            return CommandResult.Continue;
        }

        /// <summary>
        /// Handles the 'reject' command
        /// </summary>
        /// <param name="s">Unused</param>
        /// <returns>Exit</returns>
        CommandResult Reject(string s)
        {
            Debug.WriteLine("Ship layout rejected.");
            return CommandResult.Exit;
        }

        /// <summary>
        /// Handles the 'fire' command - sends out the next shot
        /// </summary>
        /// <param name="s">Unused</param>
        /// <returns>Continue</returns>
        CommandResult Fire(string s)
        {
            Debug.WriteLine("Fire command received.");
            
            Position p = _player.Offense.Fire();
            Console.WriteLine(p.ToString());
            Console.Out.Flush();
#if DEBUG
            _player.Log(">> " + p.ToString());
#endif

            return CommandResult.Continue;
        }

        /// <summary>
        /// Handles the 'hit' command - records last coordinates shot at as a hit
        /// </summary>
        /// <param name="s">Unused</param>
        /// <returns>Continue</returns>
        CommandResult Hit(string s)
        {
            Debug.WriteLine("Hit command received.");
            _player.Offense.Hit();
            return CommandResult.Continue;
        }

        /// <summary>
        /// Handles the 'miss' command - records the last coordinates shot at as a miss
        /// </summary>
        /// <param name="s">Unused</param>
        /// <returns>Continue</returns>
        CommandResult Miss(string s)
        {
            Debug.WriteLine("Miss command received.");
            _player.Offense.Miss();
            return CommandResult.Continue;
        }

        /// <summary>
        /// Handles the 'sink' command - records a ship as sunk
        /// </summary>
        /// <param name="s">Ship type</param>
        /// <returns>Continue</returns>
        CommandResult Sink(string s)
        {
            Debug.WriteLine("Sink command received.");
            _player.Offense.Sink(s);
            return CommandResult.Continue;
        }

        /// <summary>
        /// Handles the 'incoming' command
        /// </summary>
        /// <param name="s">Incoming shot coordinate</param>
        /// <returns>Continue</returns>
        CommandResult Incoming(string s)
        {
            Debug.WriteLine("Incoming shot: " + s);
            _player.Defense.IncomingShot(Position.Parse(s));
            _player.NextTurn();

            return CommandResult.Continue;
        }

        /// <summary>
        /// Handles the 'win' command
        /// </summary>
        /// <param name="s">Unused</param>
        /// <returns>Exit</returns>
        CommandResult Win(string s)
        {
            Debug.WriteLine("You win!");
            GetEnemyPositions();            

            _player.Cleanup(GameEndState.Win);
            return CommandResult.Exit;
        }

        /// <summary>
        /// Handles the 'loss' command
        /// </summary>
        /// <param name="s">Unused</param>
        /// <returns>Exit</returns>
        CommandResult Loss(string s)
        {
            Debug.WriteLine("You lost!");
            GetEnemyPositions();

            _player.Cleanup(GameEndState.Loss);            
            return CommandResult.Exit;
        }

        /// <summary>
        /// Handles the 'tie' command
        /// </summary>
        /// <param name="s">Unused</param>
        /// <returns>Exit</returns>
        CommandResult Tie(string s)
        {
            Debug.WriteLine("Tie game.");            
            GetEnemyPositions();

            _player.Cleanup(GameEndState.Tie);
            return CommandResult.Exit;
        }

        /// <summary>
        /// Handles the 'exit' command
        /// </summary>
        /// <param name="s">Unused</param>
        /// <returns>Exit</returns>
        CommandResult Exit(string s)
        {
            Debug.WriteLine("Exit directive received.");            
            return CommandResult.Exit;
        }

        /// <summary>
        /// When a win/loss/tie command is received, it is followed with a list of enemy positions - read these
        /// </summary>
        void GetEnemyPositions()
        { 
            StringBuilder sb = new StringBuilder();
            string s = String.Empty;
            List<Ship> enemyPositions = new List<Ship>();

            while (s != null)
            {
                sb.AppendLine(s);
                s = Console.ReadLine();

                if (s != null)
                {
#if DEBUG
                    _player.Log("<< " + s);
#endif
                    enemyPositions.Add(Ship.Parse(s));
                    if (enemyPositions.Count == _shipCount)
                        break;
                }
            }
            Debug.WriteLine(sb.ToString());
            _player.Offense.RegisterEnemyPositions(enemyPositions);
        }

        #endregion
    }
}
