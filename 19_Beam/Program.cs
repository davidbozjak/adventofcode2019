using SantasToolbox;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _19_Beam
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputParser = new SingleLineStringInputParser<long>(long.TryParse);
            using var inputProvider = new InputProvider<long>("Input.txt", inputParser.GetValue);

            //Part1(inputProvider.ToList());
            
            //inputProvider.Reset();

            Part2(inputProvider.ToList());
        }

        private static void Part1(IList<long> programCode)
        {
            var memory = new IntCodeMemory(programCode);
            var computer = new IntCodeComputer();

            int inBeamCount = 0;

            var world = new World();
            var printer = new WorldPrinter();

            for (int x = 0; x < 50; x++)
            {
                for (int y = 0; y < 50; y++)
                {
                    int inputcount = 0;
                    computer.Run(memory, Input, Output);

                    long Input()
                    {
                        if (inputcount++ == 0)
                        {
                            return x;
                        }
                        else
                        {
                            return y;
                        }
                    }

                    void Output(long value)
                    {
                        var tile = world.GetTile(x, y);

                        if (value == 1)
                        {
                            inBeamCount++;
                            tile.CharRepresentation = '#';
                        }
                    }
                }
            }

            printer.Print(world);
            Console.WriteLine($"In beam count: {inBeamCount}");
        }

        private static void Part2(IList<long> programCode)
        {
            var memory = new IntCodeMemory(programCode);
            var computer = new IntCodeComputer();

            var world = new World();
            var printer = new WorldPrinter();
            int furthestOutX = 1;

            for (int y = 0; ; y++)
            {
                int mostLeftHitInPreviousRow = 0;
                int mostRightHitInPreviousRow = 0;

                var hitsForPreviousRow = GetHitsForRow(y - 1);
                if (hitsForPreviousRow.Any())
                {
                    mostLeftHitInPreviousRow = hitsForPreviousRow.Select(w => w.Position.X).Min();
                    mostRightHitInPreviousRow = hitsForPreviousRow.Select(w => w.Position.X).Max();
                }

                int beamWidthInRow = 0;
                bool wasXaHit = false;
                for (int x = Math.Max(0, mostLeftHitInPreviousRow - 5); wasXaHit || x < mostRightHitInPreviousRow + 10 ; x++)
                {
                    wasXaHit = false;
                    int inputcount = 0;
                    long output = -1;

                    computer.Run(memory, Input, Output);

                    if (output == 1)
                    {
                        if (x > furthestOutX)
                            furthestOutX = x;

                        wasXaHit = true;
                        beamWidthInRow++;
                    }
                    else
                    {
                        if (output != 0) throw new Exception();
                    }

                    long Input()
                    {
                        if (inputcount++ == 0)
                        {
                            return x;
                        }
                        else
                        {
                            return y;
                        }
                    }

                    void Output(long value)
                    {
                        output = value;

                        var tile = world.GetTile(x, y);

                        if (value == 1)
                        {
                            tile.CharRepresentation = '#';
                        }
                    }
                }

                if (beamWidthInRow >= 100)
                {
                    if (IsBox100x100(y, world.Clone()))
                        return;
                }
            }

            IEnumerable<IWorldObject> GetHitsForRow(int y)
            {
                return world.WorldObjects.Where(w => w.CharRepresentation == '#' && w.Position.Y == y);
            }

            bool IsBox100x100(int y, World world)
            {
                int leftMostX = GetHitsForRow(y).Select(w => w.Position.X).Min();

                var topLeft = world.GetTile(leftMostX, y - 99);
                var topRight = world.GetTile(leftMostX + 99, y - 99);
                var bottomLeft = world.GetTile(leftMostX, y);
                var bottomRight = world.GetTile(leftMostX + 99, y);

                if (topLeft.CharRepresentation == '#' &&
                    topRight.CharRepresentation == '#' &&
                    bottomLeft.CharRepresentation == '#' &&
                    bottomRight.CharRepresentation == '#')
                {
                    topLeft.CharRepresentation = 'O';
                    topRight.CharRepresentation = 'x';
                    bottomLeft.CharRepresentation = 'x';
                    bottomRight.CharRepresentation = 'x';

                    printer.Print(world, topLeft.Position.X - 10, topRight.Position.X + 10, topLeft.Position.Y - 10, bottomLeft.Position.Y + 10);

                    Console.WriteLine($"Found topleft: {topLeft.Position}");
                    Console.WriteLine($"Result: {topLeft.Position.X * 10000 + topLeft.Position.Y}");

                    return true;
                }


                return false;
            }
        }
    }

    class Tile : IWorldObject
    {
        public Point Position { get; }

        public char CharRepresentation { get; set; }

        public int Z => 1;

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

                list.AddRange(this.tileFactory.AllCreatedInstances);
                return list;
            }
        }
        public World() { }

        private World(World world)
        {
            this.tileFactory = world.tileFactory;
        }

        public Tile GetTile(Point position) => this.tileFactory.GetOrCreateInstance(position);

        public Tile GetTile((int x, int y) position) => GetTile(new Point(position.x, position.y));

        public Tile GetTile(int x, int y) => GetTile(new Point(x, y));

        public IEnumerable<Tile> GetNeighbours(Tile tile)
        {
            yield return this.GetTile(tile.Position.Up());
            yield return this.GetTile(tile.Position.Down());
            yield return this.GetTile(tile.Position.Left());
            yield return this.GetTile(tile.Position.Right());
        }

        public World Clone()
        {
            return new World(this);
        }
    }
}
