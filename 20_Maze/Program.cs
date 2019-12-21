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

                    world.Gates.Add(new Gate(gateLabel, gateEntrance));
                }
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
            private set => this.PositionTile.CharRepresentation = value;
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
