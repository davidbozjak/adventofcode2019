using SantasToolbox;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace _24_GameOfBugs
{
    class Program
    {
        static void Main(string[] args)
        {
            using var inputProvider = new InputProvider<string>("Input.txt", (string? input, out string value) => { var isnull = string.IsNullOrWhiteSpace(input); value = input ?? string.Empty; return !isnull; });

            Part1(inputProvider);

            Console.WriteLine();
            inputProvider.Reset();

            Part2(inputProvider);
        }

        private static void Part1(IEnumerable<string> input)
        {
            using var world = ParseWorld(input);

            var bioDiversityLog = new List<int>();

            var printer = new WorldPrinter();

            while (!bioDiversityLog.Contains(world.BioDivirsetyScore))
            {
                bioDiversityLog.Add(world.BioDivirsetyScore);

                world.SimulateWorldMinute();
            }

            Console.WriteLine($"Part 1: First repeating bio diversity score: {world.BioDivirsetyScore}");
        }

        private static void Part2(IEnumerable<string> input)
        {
        }

        private static World ParseWorld(IEnumerable<string> input)
        {
            var world = new World();

            int y = 0;
            foreach (var row in input)
            {
                int x = 0;
                foreach (char c in row)
                {
                    var tile = world.GetTile(x++, y);
                    tile.IsInfested = c == '#';
                }

                y++;
            }

            return world;
        }
    }

    class Tile : IWorldObject
    {
        public Point Position { get; }

        public char CharRepresentation => this.IsInfested ? '#' : '.';

        public int Z => 1;

        public Tile(Point position)
        {
            this.Position = position;
        }

        public bool IsInfested { get; set; }
    }

    class World : IWorld, IDisposable
    {
        private readonly UniqueFactory<Point, Tile> tileFactory = new UniqueFactory<Point, Tile>(p => new Tile(p));
        private readonly Cached<int> bioDiversityCached;

        public IEnumerable<IWorldObject> WorldObjects
        {
            get
            {
                var list = new List<IWorldObject>();

                list.AddRange(this.tileFactory.AllCreatedInstances);
                return list;
            }
        }

        public World()
        {
            this.bioDiversityCached = new Cached<int>(GetBioDivirsetyScore);
        }

        private World(World world)
        {
            this.tileFactory = world.tileFactory;
            this.bioDiversityCached = world.bioDiversityCached;
        }

        public Tile GetTile(Point position) => this.tileFactory.GetOrCreateInstance(position);

        public Tile GetTile((int x, int y) position) => GetTile(new Point(position.x, position.y));

        public Tile GetTile(int x, int y) => GetTile(new Point(x, y));

        public Tile? GetTileIfExists(Point position)
        {
            return this.tileFactory.AllCreatedInstances.FirstOrDefault(w => w.Position == position);
        }

        public IEnumerable<Tile> GetNeighbours(Tile tile)
        {
            var up = this.GetTileIfExists(tile.Position.Up());
            if (up != null) yield return up;

            var down = this.GetTileIfExists(tile.Position.Down());
            if (down != null) yield return down;

            var left = this.GetTileIfExists(tile.Position.Left());
            if (left != null) yield return left;

            var right = this.GetTileIfExists(tile.Position.Right());
            if (right != null) yield return right;
        }

        public bool AreNeighbours(Tile tile1, Tile tile2)
        {
            return GetNeighbours(tile1).Any(w => w == tile2);
        }

        public World Clone()
        {
            return new World(this);
        }

        public int BioDivirsetyScore => this.bioDiversityCached.Value;

        public void SimulateWorldMinute()
        {
            var newValues = new Dictionary<Tile, bool>();

            foreach (var tile in this.tileFactory.AllCreatedInstances)
            {
                var infestedNeighbors = this.GetNeighbours(tile).Where(w => w.IsInfested).Count();

                bool newValue = tile.IsInfested;

                if (tile.IsInfested)
                {
                    newValue = infestedNeighbors == 1;
                }
                else if (infestedNeighbors == 1 || infestedNeighbors == 2)
                {
                    newValue = true;
                }

                newValues[tile] = newValue;
            }

            newValues.Keys.ToList().ForEach(w => w.IsInfested = newValues[w]);

            this.bioDiversityCached.Reset();
        }

        private int GetBioDivirsetyScore()
        {
            var infestedTiles = this.tileFactory.AllCreatedInstances.Where(w => w.IsInfested);

            int bioDiversity = 0;

            foreach (var tile in infestedTiles)
            {
                var current = tile.Position.ReadingOrder();
                var noTilesBefore = this.tileFactory.AllCreatedInstances.Where(w => w.Position.ReadingOrder() < current).Count();

                bioDiversity += (int)Math.Pow(2, noTilesBefore);
            }

            return bioDiversity;
        }

        public void Dispose()
        {
            this.bioDiversityCached.Dispose();
        }
    }
}
