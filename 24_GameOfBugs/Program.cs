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

            inputProvider.Reset();
            Console.ReadKey();

            Part2(inputProvider);
        }

        private static void Part1(IEnumerable<string> input)
        {
            using var world = new World();
            PopulateWorld(input, world);

            var bioDiversityLog = new List<int>();

            var printer = new WorldPrinter();

            while (!bioDiversityLog.Contains(world.BioDivirsetyScore))
            {
                bioDiversityLog.Add(world.BioDivirsetyScore);

                world.SimulateWorldMinute();
            }

            printer.Print(world);
            Console.WriteLine($"Part 1: First repeating bio diversity score: {world.BioDivirsetyScore}");
        }

        private static void Part2(IEnumerable<string> input)
        {
            using var world = new World(null, null);
            PopulateWorld(input, world);

            for (int tick = 0; tick < 200; tick++)
            {
                world.SimulateWorldMinute();
            }

            Console.WriteLine($"Part 2: Total bugs discovered: {world.TotalBugsInWorld} on {world.GetTotalReachableLevels()} levels");
        }

        private static void PopulateWorld(IEnumerable<string> input, World world)
        {
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
        }
    }

    class Tile : IWorldObject
    {
        public Point Position { get; }

        public virtual char CharRepresentation => this.IsInfested ? '#' : '.';

        public int Z => 1;

        public Tile(Point position)
        {
            this.Position = position;
        }

        public virtual bool IsInfested { get; set; }
    }

    class RecursiveTile : Tile
    {
        public RecursiveTile(Point position)
            : base(position)
        { }

        public override char CharRepresentation => '?';

        public override bool IsInfested
        {
            get => false;
            set
            {
                //recursive value never gets set, by design
            }
        }
    }

    class World : IWorld, IDisposable
    {
        private readonly UniqueFactory<Point, Tile> tileFactory = new UniqueFactory<Point, Tile>(p => new Tile(p));
        private readonly Cached<int> bioDiversityCached;
        private readonly Cached<int> totalBugsInSystemCached;
        private readonly bool isRecursive;
        private World? recursiveWorld;
        private World? parentWorld;
        private bool isDisposing = false;

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
            this.totalBugsInSystemCached = new Cached<int>(GetTotalBugsInSystem);
            this.isRecursive = false;

            // pre-generate all tiles
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    this.GetTile(x, y);
                }
            }
        }

        public World(World? parentWorld, World? recursiveWorld)
            : this()
        {
            this.recursiveWorld = recursiveWorld;
            this.parentWorld = parentWorld;
            this.isRecursive = true;
            this.tileFactory.InsertSpecialInstance(new Point(2, 2), new RecursiveTile(new Point(2, 2)));
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

            if (this.parentWorld != null)
            {
                // top edge, include above
                if (tile.Position.Y == 0)
                {
                    yield return this.parentWorld.GetTile(2, 1);
                }

                // bottom edge, include below
                if (tile.Position.Y == 4)
                {
                    yield return this.parentWorld.GetTile(2, 3);
                }

                // left edge, include left
                if (tile.Position.X == 0)
                {
                    yield return this.parentWorld.GetTile(1, 2);
                }

                // right edge, include right
                if (tile.Position.X == 4)
                {
                    yield return this.parentWorld.GetTile(3, 2);
                }
            }

            if (this.recursiveWorld != null)
            {
                // above
                if (tile.Position.X == 2 && tile.Position.Y == 1)
                {
                    yield return this.recursiveWorld.GetTile(0, 0);
                    yield return this.recursiveWorld.GetTile(1, 0);
                    yield return this.recursiveWorld.GetTile(2, 0);
                    yield return this.recursiveWorld.GetTile(3, 0);
                    yield return this.recursiveWorld.GetTile(4, 0);
                }

                // below
                if (tile.Position.X == 2 && tile.Position.Y == 3)
                {
                    yield return this.recursiveWorld.GetTile(0, 4);
                    yield return this.recursiveWorld.GetTile(1, 4);
                    yield return this.recursiveWorld.GetTile(2, 4);
                    yield return this.recursiveWorld.GetTile(3, 4);
                    yield return this.recursiveWorld.GetTile(4, 4);
                }

                // left
                if (tile.Position.X == 1 && tile.Position.Y == 2)
                {
                    yield return this.recursiveWorld.GetTile(0, 0);
                    yield return this.recursiveWorld.GetTile(0, 1);
                    yield return this.recursiveWorld.GetTile(0, 2);
                    yield return this.recursiveWorld.GetTile(0, 3);
                    yield return this.recursiveWorld.GetTile(0, 4);
                }

                // right
                if (tile.Position.X == 3 && tile.Position.Y == 2)
                {
                    yield return this.recursiveWorld.GetTile(4, 0);
                    yield return this.recursiveWorld.GetTile(4, 1);
                    yield return this.recursiveWorld.GetTile(4, 2);
                    yield return this.recursiveWorld.GetTile(4, 3);
                    yield return this.recursiveWorld.GetTile(4, 4);
                }
            }
        }

        public bool AreNeighbours(Tile tile1, Tile tile2)
        {
            return GetNeighbours(tile1).Any(w => w == tile2);
        }

        public int BioDivirsetyScore => this.bioDiversityCached.Value;

        public int TotalBugsInWorld => this.totalBugsInSystemCached.Value;

        public void SimulateWorldMinute(World? callingWorld = null)
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

            if (this.isRecursive)
            {
                if (this.recursiveWorld == null)
                {
                    if (this.GetNeighbours(this.GetTile(2, 2)).Where(w => w.IsInfested).Any())
                    {
                        this.recursiveWorld = new World(parentWorld: this, recursiveWorld: null);
                        this.recursiveWorld.SimulateWorldMinute(this);
                    }
                }
                else if (this.recursiveWorld != callingWorld)
                {
                    this.recursiveWorld.SimulateWorldMinute(this);
                }

                if (this.parentWorld == null)
                {
                    if (this.GetNeighbours(this.GetTile(2, 2)).Where(w => w.IsInfested).Any())
                    {
                        this.parentWorld = new World(parentWorld: null, recursiveWorld: this);
                        this.parentWorld.SimulateWorldMinute(this);
                    }
                }
                else if (this.parentWorld != callingWorld)
                {
                    this.parentWorld.SimulateWorldMinute(this);
                }
            }

            newValues.Keys.ToList().ForEach(w => w.IsInfested = newValues[w]);

            this.bioDiversityCached.Reset();
            this.totalBugsInSystemCached.Reset();
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

        private int GetTotalBugsInSystem()
        {
            return this.GetBugsInThisSystem() +
                (this.recursiveWorld?.GetBugsInThisSystemAndBelow() ?? 0) +
                (this.parentWorld?.GetBugsInThisSystemAndAbove() ?? 0);
        }

        private int GetBugsInThisSystemAndAbove()
        {
            return GetBugsInThisSystem() +
                (this.parentWorld?.GetBugsInThisSystemAndAbove() ?? 0);
        }

        private int GetBugsInThisSystemAndBelow()
        {
            return GetBugsInThisSystem() +
                (this.recursiveWorld?.GetBugsInThisSystemAndBelow() ?? 0);
        }

        private int GetBugsInThisSystem()
        {
            return this.tileFactory.AllCreatedInstances.Where(w => w.IsInfested).Count();
        }

        public int GetTotalReachableLevels()
        {
            return 1 + 
                (this.parentWorld?.GetReachableLevelsAbove() ?? 0) + 
                (this.recursiveWorld?.GetReachableLevelsBelow() ?? 0);
        }

        private int GetReachableLevelsAbove()
        {
            return 1 + (this.parentWorld?.GetReachableLevelsAbove() ?? 0);
        }

        private int GetReachableLevelsBelow()
        {
            return 1 + (this.recursiveWorld?.GetReachableLevelsBelow() ?? 0);
        }

        public void Dispose()
        {
            if (!this.isDisposing)
            {
                this.isDisposing = true;

                this.bioDiversityCached.Dispose();
                this.totalBugsInSystemCached.Dispose();

                this.recursiveWorld?.Dispose();
                this.parentWorld?.Dispose();
            }
        }
    }
}
