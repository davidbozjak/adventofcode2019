using SantasToolbox;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace _11_Robot
{
    class Program
    {
        static void Main(string[] _)
        {
            var inputParser = new SingleLineStringInputParser<long>(long.TryParse);
            using var inputProvider = new InputProvider<long>("Input.txt", inputParser.GetValue);

            Part1(inputProvider.ToList());

            Console.WriteLine("Press any key to continue to Part2");
            Console.ReadKey();

            inputProvider.Reset();

            Part2(inputProvider.ToList());
        }

        private static void Part1(IList<long> programCode)
        {
            var world = RunPaintingRobot(programCode, 0);

            Console.WriteLine($"Part 1 Done.");
            Console.WriteLine($"Number of visited pannels: {world.WorldObjects.Count() - 1}");
        }

        private static void Part2(IList<long> programCode)
        {
            RunPaintingRobot(programCode, 1);

            Console.WriteLine($"Part 2 Done.");
        }

        private static World RunPaintingRobot(IList<long> programCode, int startingColor)
        {
            var computer = new IntCodeComputer();
            var memory = new IntCodeMemory(programCode);

            var world = new World();
            var robot = new Robot(world);
            world.Robot = robot;
            robot.TileAtPosition.Paint = startingColor;

            var printer = new WorldPrinter();

            int outputCount = 0;

            var memoryAfterExecution = computer.Run(memory, RobotInput, RobotOutput);

            printer.Print(world);

            return world;

            long RobotInput()
            {
                //Task.Delay(30).Wait();
                return robot.TileAtPosition.Paint;
            }

            void RobotOutput(long output)
            {
                if (outputCount++ % 2 == 0)
                {
                    world.GetTile(robot.Position).Paint = (int)output;
                }
                else
                {
                    robot.Turn((int)output);
                    robot.MoveOneStep();
                }

                //printer.Print(world, robot);
            }
        }

        class Robot : IWorldObject
        {
            private readonly World world;

            public Robot(World world)
            {
                this.world = world;
                this.TileAtPosition = this.world.GetTile(new Point(0, 0));
            }

            public Tile TileAtPosition { get; private set; }

            public Point Position => this.TileAtPosition.Position;

            private int orientation;
            public int Orientation
            {
                get => this.orientation;
                set
                {
                    while (value >= 4) value -= 4;
                    while (value < 0) value += 4;
                    this.orientation = value;
                }
            }

            public char CharRepresentation => orientation switch
            {
                0 => '^',
                1 => '>',
                2 => 'v',
                3 => '<',
                _ => throw new Exception()
            };

            public int Z => 2;

            public void Turn(int input)
            {
                if (input == 0)
                {
                    // turning left
                    this.Orientation--;
                }
                else if (input == 1)
                {
                    // turning right
                    this.Orientation++;
                }
            }

            public void MoveOneStep()
            {
                var newPosition = this.Orientation switch
                {
                    0 => this.Position.Up(),
                    1 => this.Position.Right(),
                    2 => this.Position.Down(),
                    3 => this.Position.Left(),
                    _ => throw new Exception()
                };

                this.TileAtPosition = this.world.GetTile(newPosition);
            }
        }

        class Tile : IWorldObject
        {
            public Point Position { get; }

            public char CharRepresentation => this.Paint switch
            {
                0 => '.',
                1 => '#',
                _ => throw new Exception()
            };

            public int Z => 1;

            public int Paint { get; set; }

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

            public Robot? Robot { get; set; }
        }
    }
}