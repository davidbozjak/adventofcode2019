using SantasToolbox;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace _13_Bricks
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

            Stack<long> outputs = new Stack<long>();

            var world = new World();

            computer.Run(memory, null, Output);

            var printer = new WorldPrinter();
            printer.Print(world);

            var blockTiles = world.WorldObjects.Where(w => w.CharRepresentation == 'x');

            Console.WriteLine($"Number of block tiles is {blockTiles.Count()}");

            void Output(long output)
            {
                outputs.Push(output);

                if (outputs.Count == 3)
                {
                    int id = (int)outputs.Pop();
                    int y = (int)outputs.Pop();
                    int x = (int)outputs.Pop();

                    var tile = world.GetTile(new Point(x, y));
                    tile.Paint = id;
                }
            }
        }

        private static void Part2(IList<long> programCode)
        {
            var memory = new IntCodeMemory(programCode);
            memory[0] = 2;
            var computer = new IntCodeComputer();

            Stack<long> outputs = new Stack<long>();

            var world = new World();
            var printer = new WorldPrinter();
            long score = 0;

            computer.Run(memory, Input, Output);

            printer.Print(world);

            Console.WriteLine($"Score is {score}");

            void Output(long output)
            {
                outputs.Push(output);

                if (outputs.Count == 3)
                {
                    int id = (int)outputs.Pop();
                    int y = (int)outputs.Pop();
                    int x = (int)outputs.Pop();

                    if (x == -1 && y == 0)
                    {
                        score = id;
                    }
                    else
                    {
                        var tile = world.GetTile(new Point(x, y));
                        tile.Paint = id;
                    }
                }
            }

            long Input()
            {
                //printer.Print(world);

                //Console.WriteLine($"Score is {score}");
                //Task.Delay(20).Wait();

                var ball = world.WorldObjects.First(w => w.CharRepresentation == 'o');
                var paddle = world.WorldObjects.First(w => w.CharRepresentation == '_');

                return ball.Position.X.CompareTo(paddle.Position.X);
            }

        }

        class Tile : IWorldObject
        {
            public Point Position { get; }

            public char CharRepresentation => this.Paint switch
            {
                0 => ' ',
                1 => '#',
                2 => 'x',
                3 => '_',
                4 => 'o',
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

            public IEnumerable<IWorldObject> WorldObjects => this.tileFactory.AllCreatedInstances;

            public World()
            {

            }

            public Tile GetTile(Point position) => this.tileFactory.GetOrCreateInstance(position);
        }
    }
}
