﻿using SantasToolbox;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace _18_Keys
{
    class Program
    {
        static void Main(string[] args)
        {
            using var inputProvider = new InputProvider<string>("Input.txt", (string? input, out string value) => { var isnull = string.IsNullOrWhiteSpace(input); value = input ?? string.Empty; return !isnull; });

            var world = ParseWorld(inputProvider);

            var printer = new WorldPrinter();
            printer.Print(world);

            Part1(world);
        }

        private static void Part1(World world)
        {
            if (world.Entrance == null) throw new Exception("Invalidly parsed world, no entrance");

            Tile location = world.Entrance;

            Console.WriteLine($"Keys to pick up: {world.Keys.Count}");

            int steps = GetStepsToFinish(location, world);

            Console.WriteLine($"Picked up all keys after {steps} steps");
        }

        static int maxPrintedLevel = 0;
        static Dictionary<string, int> alreadyComputedSubProblems = new Dictionary<string, int>();

        private static int GetStepsToFinish(Tile location, World world, IEnumerable<Tile>? pathToTake = null)
        {
            int steps = 0;

            if (pathToTake != null)
            {
                WalkPath(pathToTake);
            }

            if (world.Keys.Count > 0)
            {
                var options = world.Keys.Select(w => (w.Identifier, GetPath(w.PositionTile))).Where(w => w.Item2 != null).ToList();
                
                var strOptions = string.Join(",", options.Select(w => w.Identifier).OrderBy(w => w));
                var strGates = string.Join(",", world.Gates.Select(w => w.Identifier).OrderBy(w => w));

                var key = $"{location.Position} Options:[{strOptions}] Gates:[{strGates}]";

                if (alreadyComputedSubProblems.ContainsKey(key))
                    return steps + alreadyComputedSubProblems[key];

                int bestOption = int.MaxValue;
                foreach (var (identifier, option) in options)
                {
                    int stepsForOption = GetStepsToFinish(location, world.Clone(), option);

                    if (stepsForOption < bestOption)
                    {
                        bestOption = stepsForOption;
                    }
                }

                steps += bestOption;

                alreadyComputedSubProblems[key] = bestOption;
            }
            
            return steps;

            void WalkPath(IEnumerable<Tile> path)
            {
                foreach (var tile in path)
                {
                    var key = world.KeyOrDefault(tile);
                    if (key != null)
                    {
                        world.Unlock(key);
                    }

                    var gate = world.GateOrDefault(tile);
                    if (gate != null) throw new Exception($"Invalid path, gate {gate.Identifier} is locked!");

                    location = tile;
                    steps++;
                }
            }

            IList<Tile>? GetPath(Tile target)
            {
                var start = location;

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

                    foreach (var neighbour in Neighbours(current))
                    {
                        if (world.GateOrDefault(neighbour) != null) continue;
                        if (neighbour.IsWall) continue;

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

                IEnumerable<Tile> Neighbours(Tile tile)
                {
                    yield return world.GetTile(tile.Position.Up());
                    yield return world.GetTile(tile.Position.Down());
                    yield return world.GetTile(tile.Position.Left());
                    yield return world.GetTile(tile.Position.Right());
                }

                int Heuristic(Tile tile)
                {
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
        }

        private static World ParseWorld(IEnumerator<string> input)
        {
            var world = new World(true);

            for (int y = 0; input.MoveNext(); y++)
            {
                string row = input.Current;

                int x = 0;
                foreach (var tileChar in row)
                {
                    var tile = world.GetTile(x, y);
                    tile.CharRepresentation = tileChar;

                    if (tileChar == '#')
                    {
                        tile.IsWall = true;
                    }
                    else if (tileChar >= 'a' && tileChar <= 'z')
                    {
                        var key = new Key(tileChar, tile);
                        world.Keys.Add(key);
                    }
                    else if (tileChar >= 'A' && tileChar <= 'Z')
                    {
                        var gate = new Gate(tileChar, tile);
                        world.Gates.Add(gate);
                    }
                    else if (tileChar == '@')
                    {
                        world.Entrance = tile;
                    }

                    x++;
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

        public Gate(char identifier, Tile positionTile)
        {
            this.Identifier = new string(identifier, 1).ToUpper();
            this.PositionTile = positionTile;
        }
    }

    class Key : IWorldObject
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

        public Key(char identifier, Tile positionTile)
        {
            this.Identifier = new string(identifier, 1).ToUpper();
            this.PositionTile = positionTile;
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

        public bool IsWall { get; set; }
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

        public List<Key> Keys { get; } = new List<Key>();

        public List<Gate> Gates { get; } = new List<Gate>();

        public bool PrintMessages { get; } = false;

        public World(bool printMessages)
        {
            this.PrintMessages = printMessages;
        }

        private World(World world)
        {
            this.tileFactory = world.tileFactory;

            this.Keys = world.Keys.ToList();
            this.Gates = world.Gates.ToList();

            this.PrintMessages = false;
        }

        public Tile GetTile(Point position) => this.tileFactory.GetOrCreateInstance(position);

        public Tile GetTile((int x, int y) position) => GetTile(new Point(position.x, position.y));

        public Tile GetTile(int x, int y) => GetTile(new Point(x, y));

        public Gate? GateOrDefault(Tile tile) => this.Gates.FirstOrDefault(w => w.PositionTile == tile);

        public Key? KeyOrDefault(Tile tile) => this.Keys.FirstOrDefault(w => w.PositionTile == tile);

        public void Unlock(Key key)
        {
            this.Keys.Remove(key);

            if (this.PrintMessages)
            {
                Console.WriteLine($"Picked up key {key.Identifier}");
            }

            var gate = this.Gates.FirstOrDefault(w => w.Identifier == key.Identifier);

            if (gate != null)
            {
                if (this.PrintMessages)
                {
                    Console.WriteLine($"Unlocked gate {gate.Identifier}");
                }

                this.Gates.Remove(gate);
            }
        }

        public World Clone()
        {
            return new World(this);
        }
    }
}
