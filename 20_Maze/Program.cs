using SantasToolbox;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace _20_Maze
{
    class Program
    {
        static void Main(string[] args)
        {
            using var inputProvider = new InputProvider<string>("Input.txt", (string? input, out string value) => { var isnull = string.IsNullOrWhiteSpace(input); value = input ?? string.Empty; return !isnull; });

            var world = ParseWorld(inputProvider, "AA", "ZZ");

            var printer = new WorldPrinter();
            printer.Print(world);

            Part1(world);
        }

        private static void Part1(World world)
        {
            if (world.Entrance == null) throw new ArgumentException();
            if (world.Exit == null) throw new ArgumentException();

            var path = GetPath(world.Entrance, world.Exit, world);

            if (path == null)
            {
                Console.WriteLine("Fail :(");
            }
            else
            {
                path.ToList().ForEach(w => w.CharRepresentation = 'o');

                var printer = new WorldPrinter();
                printer.Print(world);

                Console.WriteLine("Part 1 Complete");
                Console.WriteLine($"Path length: {path.Count}");
            }
        }

        private static IList<Tile>? GetPath(Tile start, Tile target, World world)
        {
            var cameFrom = new Dictionary<Tile, Tile>();
            var openSet = new List<Tile> { start };

            var gScore = new Dictionary<Tile, int>();
            gScore[start] = 0;

            var fScore = new Dictionary<Tile, int>();
            fScore[start] = Heuristic(start);

            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(w => fScore[w]).First();

                if (current == target)
                {
                    return ReconstructPath(current);
                }

                openSet.Remove(current);

                foreach (var neighbour in world.GetNeighbours(current))
                {
                    if (!neighbour.IsWalkable) continue;

                    var tentativeScore = gScore[current] + 1;

                    if (!gScore.ContainsKey(neighbour) || tentativeScore < gScore[neighbour])
                    {
                        cameFrom[neighbour] = current;
                        gScore[neighbour] = tentativeScore;
                        fScore[neighbour] = tentativeScore + Heuristic(neighbour);

                        if (!openSet.Contains(neighbour))
                        {
                            openSet.Add(neighbour);
                        }
                    }
                }
            }

            // Not found!
            return null;

            int Heuristic(Tile tile)
            {
                // make heuristic always prefer gates in the world
                if (world.GateOrDefault(tile) != null) return 0;

                // return menhattan distance as heuristic
                return Math.Abs(tile.Position.X - target.Position.X) +
                    Math.Abs(tile.Position.Y - target.Position.Y);
            }

            IList<Tile> ReconstructPath(Tile current)
            {
                var path = new List<Tile> { current };
                while (cameFrom.ContainsKey(current))
                {
                    path.Add(cameFrom[current]);
                    current = cameFrom[current];
                }

                path.Reverse();

                return path.Skip(1).ToList();
            }
        }

        private static World ParseWorld(IEnumerator<string> input, string entranceLabel, string exitLabel)
        {
            var world = new World();

            var gateTiles = new List<Tile>();

            for (int y = 0; input.MoveNext(); y++)
            {
                string row = input.Current;

                int x = 0;
                foreach (var tileChar in row)
                {
                    var tile = world.GetTile(x, y);
                    tile.CharRepresentation = tileChar;

                    if (tileChar == '.')
                    {
                        tile.IsWalkable = true;
                    }
                    else if (tileChar >= 'A' && tileChar <= 'Z')
                    {
                        gateTiles.Add(tile);
                    }

                    x++;
                }
            }

            // find the gates
            for (int i = 0; i < gateTiles.Count; i++)
            {
                var tile1 = gateTiles[i];

                for (int j = 0; j < gateTiles.Count; j++)
                {
                    var tile2 = gateTiles[j];

                    if (tile1 == tile2) continue;

                    if (!world.AreNeighbours(tile1, tile2)) continue;

                    (Tile first, Tile second) = tile1.Position.ReadingOrder() > tile2.Position.ReadingOrder() ?
                        (tile1, tile2) : (tile2, tile1);

                    string gateLabel = first.CharRepresentation.ToString() + second.CharRepresentation.ToString();

                    var walkableNeighbours = world.GetNeighbours(tile1).Concat(world.GetNeighbours(tile2)).
                        Where(w => w.IsWalkable).ToList();

                    if (walkableNeighbours.Count != 1) throw new Exception("Invalid assumption, more than 1 tile");

                    var gateEntrance = walkableNeighbours[0];

                    if (world.Gates.Any(w => w.Identifier == gateLabel && w.PositionTile == gateEntrance))
                        continue;

                    world.Gates.Add(new Gate(gateLabel, gateEntrance));
                }
            }

            var entrance = world.Gates.Where(w => w.Identifier == entranceLabel).First();
            world.Entrance = entrance.PositionTile;
            world.Gates.Remove(entrance);

            var exit = world.Gates.Where(w => w.Identifier == exitLabel).First();
            world.Exit = exit.PositionTile;
            world.Gates.Remove(exit);

            for (int i = 0; i < world.Gates.Count; i++)
            {
                var gateTile = world.Gates[i];
                gateTile.CharRepresentation = (char)(i + 'a');
            }

            return world;
        }
    }

    class Gate : IWorldObject
    {
        public string Identifier { get; }

        public Tile PositionTile { get; }

        public Point Position => this.PositionTile.Position;

        public char CharRepresentation
        {
            get => this.PositionTile.CharRepresentation;
            set => this.PositionTile.CharRepresentation = value;
        }

        public int Z => 2;

        public Gate(string identifier, Tile positionTile)
        {
            this.Identifier = identifier;
            this.PositionTile = positionTile;
            this.CharRepresentation = 'x';
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

        public bool IsWalkable { get; set; } = false;
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

        public Tile? Entrance { get; set; }

        public Tile? Exit { get; set; }

        public List<Gate> Gates { get; } = new List<Gate>();

        public World()
        {
        }

        private World(World world)
        {
            this.tileFactory = world.tileFactory;

            this.Gates = world.Gates.ToList();
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

            var gate = this.GateOrDefault(tile);

            if (gate != null)
            {
                var otherGate = this.Gates
                    .Where(w => w != gate)
                    .Where(w => w.Identifier == gate.Identifier)
                    .First();
                yield return otherGate.PositionTile;
            }
        }

        public bool AreNeighbours(Tile tile1, Tile tile2)
        {
            return GetNeighbours(tile1).Any(w => w == tile2);
        }

        public Gate? GateOrDefault(Tile tile) => this.Gates.FirstOrDefault(w => w.PositionTile == tile);

        public World Clone()
        {
            return new World(this);
        }
    }
}
