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

            Solve(world, false);
            Console.ReadKey();
            Solve(world, true);
        }

        private static void Solve(World world, bool isRecursive)
        {
            if (world.Entrance == null) throw new ArgumentException();
            if (world.Exit == null) throw new ArgumentException();

            var path = GetPath(isRecursive, world.Entrance, world.Exit, world);

            var printer = new WorldPrinter();
            
            if (path == null)
            {
                printer.Print(world);
                Console.WriteLine("Fail :(");
            }
            else
            {
                path.ToList().ForEach(w => w.CharRepresentation = 'o');
                printer.Print(world);

                Console.WriteLine("Part 2 Complete");
                Console.WriteLine($"Path length: {path.Count}");
            }
        }

        private static IList<Tile>? GetPath(bool isRecursive, Tile start, Tile target, World world)
        {
            var cameFrom = new Dictionary<Tile3D, Tile3D>();
            
            var start3d = new Tile3D(start, 0);

            var openSet = new List<Tile3D> { start3d };

            var gScore = new Dictionary<Tile3D, int>();
            gScore[start3d] = 0;

            var fScore = new Dictionary<Tile3D, int>();
            fScore[start3d] = Heuristic(start3d);

            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(w => fScore[w]).First();
                
                current.Tile.CharRepresentation = 'v';

                if (current.Tile == target && (!isRecursive || (isRecursive && current.Level == 0)))
                {
                    return ReconstructPath(current);
                }

                openSet.Remove(current);

                foreach (var neighbour in world.Get3DNeighbours(current, isRecursive))
                {
                    if (!neighbour.Tile.IsWalkable) continue;

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

            int Heuristic(Tile3D tile)
            {
                var manhattanDistance = Math.Abs(tile.X - target.Position.X) +
                    Math.Abs(tile.Y - target.Position.Y);

                if (isRecursive)
                {
                    // add third dimension
                    manhattanDistance += tile.Level * 100;
                }

                return manhattanDistance;
            }

            IList<Tile> ReconstructPath(Tile3D current)
            {
                var path = new List<Tile> { current.Tile };
                while (cameFrom.ContainsKey(current))
                {
                    path.Add(cameFrom[current].Tile);
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

                    (Tile first, Tile second) = tile1.Position.ReadingOrder() < tile2.Position.ReadingOrder() ?
                        (tile1, tile2) : (tile2, tile1);

                    string gateLabel = first.CharRepresentation.ToString() + second.CharRepresentation.ToString();

                    var walkableNeighbours = world.GetNeighbours(tile1).Concat(world.GetNeighbours(tile2)).
                        Where(w => w.IsWalkable).ToList();

                    if (walkableNeighbours.Count != 1) throw new Exception("Invalid assumption, more than 1 tile");

                    var gateEntrance = walkableNeighbours[0];

                    if (world.Gates.Any(w => w.Identifier == gateLabel && w.PositionTile == gateEntrance))
                        continue;

                    var gate = new Gate(gateLabel, gateEntrance);

                    gate.IncreasingLevel = true;
                    //set to false if on edge
                    int maxX = world.WorldObjects.Select(w => w.Position.X).Max() - 1;
                    int maxY = world.WorldObjects.Select(w => w.Position.Y).Max() - 1;
                    if (tile1.Position.X == 0)
                    {
                        gate.IncreasingLevel = false;
                    }
                    else if (tile1.Position.Y == 0)
                    {
                        gate.IncreasingLevel = false;
                    }
                    else if (tile2.Position.X == maxX)
                    {
                        gate.IncreasingLevel = false;
                    }
                    else if (tile2.Position.Y == maxY)
                    {
                        gate.IncreasingLevel = false;
                    }

                    world.Gates.Add(gate);
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
                //gateTile.CharRepresentation = (char)(i + 'a');
                gateTile.CharRepresentation = gateTile.IncreasingLevel ? '+' : '-';
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

        public bool IncreasingLevel { get; set; }

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

    class Tile3D
    {
        public Tile Tile { get; }
        public int X => this.Tile.Position.X;
        public int Y => this.Tile.Position.Y;

        public int Level { get; set; }

        public Tile3D(Tile tile, int level)
        {
            this.Tile = tile;
            this.Level = level;
        }
    }

    class World : IWorld
    {
        private readonly UniqueFactory<Point, Tile> tileFactory = new UniqueFactory<Point, Tile>(p => new Tile(p));
        private readonly UniqueFactory<(int x, int y, int level), Tile3D> tile3dFactory;

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
            this.tile3dFactory = new UniqueFactory<(int x, int y, int level), Tile3D>(w => new Tile3D(this.GetTile(w.x, w.y), w.level));
        }

        private World(World world)
        {
            this.tileFactory = world.tileFactory;
            this.tile3dFactory = world.tile3dFactory;

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
        }

        public IEnumerable<Tile3D> Get3DNeighbours(Tile3D tile, bool isRecursive)
        {
            var gate = this.GateOrDefault(tile.Tile);

            if (gate != null)
            {
                if (!isRecursive || gate.IncreasingLevel || tile.Level > 0)
                {
                    Gate otherGate = GetOtherGate(gate);
                    var newLevel = tile.Level + (gate.IncreasingLevel ? 1 : -1);
                    yield return tile3dFactory.GetOrCreateInstance((otherGate.Position.X, otherGate.Position.Y, newLevel));
                }
            }

            foreach (var neighbour3d in this.GetNeighbours(tile.Tile))
            {
                yield return tile3dFactory.GetOrCreateInstance((neighbour3d.Position.X, neighbour3d.Position.Y, tile.Level));
            }
        }

        public Gate GetOtherGate(Gate gate)
        {
            return this.Gates
                    .Where(w => w != gate)
                    .Where(w => w.Identifier == gate.Identifier)
                    .First();
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
