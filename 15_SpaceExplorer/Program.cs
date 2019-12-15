using SantasToolbox;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace _15_SpaceExplorer
{
    class Program
    {
        static void Main(string[] _)
        {
            var inputParser = new SingleLineStringInputParser<long>(long.TryParse);
            using var inputProvider = new InputProvider<long>("Input.txt", inputParser.GetValue);

            Part1(inputProvider.ToList());

            inputProvider.Reset();

            Part2(inputProvider.ToList());
        }

        private static void Part1(IList<long> programCode)
        {
            var memory = new IntCodeMemory(programCode);
            var computer = new IntCodeComputer();

            var world = new World();
            var robot = new Robot(world, true);
            world.Robot = robot;

            try
            {
                computer.Run(memory, robot.GetCommand, robot.SetResponseToCommand);
            }
            catch
            {

            }
        }

        private static void Part2(IList<long> programCode)
        {
            var memory = new IntCodeMemory(programCode);
            var computer = new IntCodeComputer();

            var world = new World();
            var robot = new Robot(world, false);
            world.Robot = robot;

            var printer = new WorldPrinter(frameSize: 50);

            try
            {
                computer.Run(memory, robot.GetCommand, robot.SetResponseToCommand);
            }
            catch
            {
                
            }

            world.Robot = null; // must mean whole area is explored, removing robot

            Func<Tile, Tile>[] commands = { x => world.GetTile(x.Position.Up()), x => world.GetTile(x.Position.Down()), x => world.GetTile(x.Position.Left()), x => world.GetTile(x.Position.Right()) };

            for (int count = 1, tick = 1; count > 0; tick++)
            {
                var oxygenTiles = world.WorldObjects.Cast<Tile>().Where(w => w.Paint == 2).ToList();
                
                int oxygenated = 0;

                foreach (var oxygenTile in oxygenTiles)
                {
                    foreach (var command in commands)
                    {
                        var tile = command(oxygenTile);

                        if (tile.Paint == 1)
                        {
                            tile.Paint = 2;
                            oxygenated++;
                        }
                    }
                }

                count = world.WorldObjects.Where(w => w.CharRepresentation == '.').Count();
                Console.WriteLine($"After {tick} non-oxygen tiles left: {count}");
            }
        }

        class Robot : IWorldObject
        {
            private readonly World world;
            private readonly bool haltWhenTargetFound;

            public Robot(World world, bool haltWhenTargetFound)
            {
                this.world = world;
                this.TileAtPosition = this.world.GetTile(new Point(0, 0));
                this.haltWhenTargetFound = haltWhenTargetFound;
            }

            public Tile TileAtPosition { get; private set; }

            public Point Position => this.TileAtPosition.Position;

            public char CharRepresentation => 'D';

            public int Z => 2;

            private readonly Stack<long> sentCommands = new Stack<long>();
            private long sentCommand;

            public long GetCommand()
            {
                bool foundUnknown = false;
                for (long command = 1; command <= 4; command++)
                {
                    var woudlBePosition = this.GetPositionFromCommand(command);

                    var tile = this.world.GetTile(woudlBePosition);

                    if (!tile.IsKnown)
                    {
                        foundUnknown = true;
                        this.sentCommands.Push(command);
                        break;
                    }
                }

                if (foundUnknown)
                {
                    sentCommand = this.sentCommands.Peek();
                }
                else
                {
                    var command = this.sentCommands.Pop();
                    var backTrackCommand = command switch
                    {
                        1 => 2,
                        2 => 1,
                        3 => 4,
                        4 => 3,
                        _ => throw new Exception()
                    };

                    sentCommand = backTrackCommand;
                }

                return sentCommand;
            }

            public WorldPrinter? Printer { get; set; }

            private int countSinceDraw;

            public void SetResponseToCommand(long response)
            {
                Point newPosition = GetPositionFromCommand(this.sentCommand);

                var tile = this.world.GetTile(newPosition);
                tile.Paint = (int)response;

                if (response == 0)
                {
                    //remove command from history, as we can't back track there.
                    this.sentCommands.Pop();
                }
                else if (response == 1 || response == 2)
                {
                    this.TileAtPosition = tile;
                }
                else throw new Exception();

                if (this.countSinceDraw++ == 20)
                {
                    if (this.Printer != null)
                    {
                        this.Printer.Print(this.world, this);
                        
                        //Console.ReadKey();
                        Task.Delay(30).Wait();
                    }

                    this.countSinceDraw = 0;
                }

                if (this.haltWhenTargetFound && response == 2)
                {
                    Console.WriteLine($"Done, found Oxygen at {this.Position}!");
                    Console.WriteLine($"Direct moves to here: {this.sentCommands.Count()}!");
                    throw new Exception("Halting mechanism :)");
                }
            }

            private Point GetPositionFromCommand(long command) => command switch
                {
                    1 => this.Position.Up(),
                    2 => this.Position.Down(),
                    3 => this.Position.Left(),
                    4 => this.Position.Right(),
                    _ => throw new Exception()
                };
        }

        class Tile : IWorldObject
        {
            public Point Position { get; }

            public char CharRepresentation => this.Paint switch
            {
                -1 => '?',
                0 => '#',
                1 => '.',
                2 => 'O',
                _ => throw new Exception()
            };

            public int Z => 1;

            public bool IsKnown { get; private set; } = false;
            public bool IsWall => this.Paint == 0;

            private int paint;
            public int Paint 
            {
                get => this.IsKnown ? this.paint : -1; 
                set
                {
                    this.IsKnown = true;
                    this.paint = value;
                }
            }

            public Tile(Point position)
            {
                this.Position = position;
            }
        }

        class World : IWorld
        {
            private readonly UniqueFactory<Point, Tile> tileFactory = new UniqueFactory<Point, Tile>(p => new Tile(p));

            public IEnumerable<IWorldObject> WorldObjects
            {
                get
                {
                    var list = new List<IWorldObject>();

                    if (this.Robot != null)
                    {
                        list.Add(this.Robot);
                    }

                    list.AddRange(this.tileFactory.AllCreatedInstances);
                    return list;
                }
            }

            public World()
            {

            }

            public Tile GetTile(Point position) => this.tileFactory.GetOrCreateInstance(position);

            public Tile GetTile((int x, int y) position) => GetTile(new Point(position.x, position.y));

            public Tile GetTile(int x, int y) => GetTile(new Point(x, y));

            public Robot? Robot { get; set; }
        }
    }
}