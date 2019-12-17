using SantasToolbox;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace _17_TBN
{
    class Program
    {
        static void Main(string[] _)
        {
            var inputParser = new SingleLineStringInputParser<long>(long.TryParse);
            using var inputProvider = new InputProvider<long>("Input.txt", inputParser.GetValue);

            var world = Part1(inputProvider.ToList());

            inputProvider.Reset();

            Part2(inputProvider.ToList(), world);
        }

        private static World Part1(IList<long> programCode)
        {
            var memory = new IntCodeMemory(programCode);
            var computer = new IntCodeComputer();

            var world = new World();
            
            int x = 0, y = 0;
            var printer = new WorldPrinter();

            computer.Run(memory, null, Output);

            printer.Print(world);

            var intersections = new List<Tile>();

            foreach (var tile in world.WorldObjects.Cast<Tile>().Where(w => w.CharRepresentation == '#'))
            {
                // tile is an intersection if it has # on all four neighbours
                var up = world.GetTile(tile.Position.Up());
                var down = world.GetTile(tile.Position.Down());
                var left = world.GetTile(tile.Position.Left());
                var right = world.GetTile(tile.Position.Right());

                if (up.CharRepresentation == '#' &&
                    down.CharRepresentation == '#' &&
                    left.CharRepresentation == '#' &&
                    right.CharRepresentation == '#')
                {
                    intersections.Add(tile);
                }
            }

            printer.Print(world);

            var sum = intersections.Sum(w => w.Position.X * w.Position.Y);

            Console.WriteLine($"Part 1: Sum of intersection point coordinates: {sum}");

            return world;

            void Output(long character)
            {
                if (character == 10)
                {
                    y++;
                    x = 0;
                }
                else
                {
                    var tile = world.GetTile(x, y).CharRepresentation = (char)character;
                    x++;
                }
            }
        }

        private static void Part2(IList<long> programCode, World world)
        {
            var memory = new IntCodeMemory(programCode);
            memory[0] = 2;

            var computer = new IntCodeComputer();

            var printer = new WorldPrinter();

            var initialPosition = world.WorldObjects.Cast<Tile>().Where(w => w.CharRepresentation == '^').First();
            initialPosition.CharRepresentation = '#';

            var tilesToVisit = world.WorldObjects.Cast<Tile>().Where(w => w.CharRepresentation == '#').ToList();

            int maxX = tilesToVisit.Select(w => w.Position.X).Max();
            int maxY = tilesToVisit.Select(w => w.Position.Y).Max();

            var roads = new List<Road>();

            // break into roads

            FindHorizontalRows(tilesToVisit, maxY, roads);
            FindVerticalRows(tilesToVisit, maxX, roads);

            printer.Print(world);

            // find intersections
            foreach (var road in roads)
            {
                //var intersectingRoads = roads.Where(w => w != road && 
                //(w.Intersection1 == road.Intersection1 || w.Intersection1 == road.Intersection2 ||
                // w.Intersection2 == road.Intersection1 || w.Intersection2 == road.Intersection2));
                //road.IntersectingRoads.AddRange(intersectingRoads);

                road.Intersection1Roads.AddRange(roads.Where(w => w != road &&
                                                                  (road.Intersection1 == w.Intersection1 || road.Intersection1 == w.Intersection2)));

                road.Intersection2Roads.AddRange(roads.Where(w => w != road &&
                                                                  (road.Intersection2 == w.Intersection1 || road.Intersection2 == w.Intersection2)));
            }


            var startingRoad = roads.Where(w => w.Intersection1 == initialPosition || w.Intersection2 == initialPosition).First();

            var allPaths = new List<List<Road>>();
            FillAllPaths(initialPosition, startingRoad, new List<Road> { startingRoad }, allPaths, tilesToVisit.Count);

            //var longestPathLengths = allPaths.Select(w => (w, w.SelectMany(road => road.Tiles).ToHashSet().Count)).OrderByDescending(w => w.Item2).Take(10).ToList();

            //foreach ((var path, var length) in longestPathLengths)
            //{
            //    Console.WriteLine($"{length}: {string.Join(", ", path.Select(w => w.Id))}");
            //}

            //var path1 = longestPathLengths[0].w;

            //foreach (var tile in tilesToVisit)
            //{
            //    if (path1.SelectMany(w => w.Tiles).Contains(tile)) continue;
            //    Console.WriteLine($"{tile.Position} : {tile.CharRepresentation}");
            //}

            var pathsThatCoverAll = allPaths.Where(w => w.SelectMany(road => road.Tiles).ToHashSet().Count == tilesToVisit.Count).ToList();

            foreach (var path in pathsThatCoverAll)
            {
                Console.WriteLine($"{string.Join(", ", path.Select(w => w.Id))}");
            }

            //long starDust = 0;
            //computer.Run(memory, null, Output);

            //printer.Print(world);


            //void Output(long collectedStarDust)
            //{
            //    starDust = collectedStarDust;
            //}
        }

        private static void FillAllPaths(Tile entryIntersection, Road road, List<Road> path, List<List<Road>> allPaths, int tilesToVisit)
        {
            bool isEndOfRoad = true;

            if (path.SelectMany(w => w.Tiles).ToHashSet().Count == tilesToVisit)
            {
                allPaths.Add(path);
                return;
            }

            List<Road> exitRoads;
            Tile exitIntersection;
            if (entryIntersection == road.Intersection1)
            {
                exitRoads = road.Intersection2Roads;
                exitIntersection = road.Intersection2;
            }
            else if (entryIntersection == road.Intersection2) 
            {
                exitRoads = road.Intersection1Roads;
                exitIntersection = road.Intersection1;
            }
            else throw new Exception();

            foreach (var nextRoad in exitRoads)
            {
                //if (path.Count(w => w == nextRoad) > 1) continue;
                if (path.Contains(nextRoad)) continue;

                isEndOfRoad = false;

                var newPath = path.ToList();
                newPath.Add(nextRoad);

                FillAllPaths(exitIntersection, nextRoad, newPath, allPaths, tilesToVisit);
            }

            if (isEndOfRoad)
            {
                allPaths.Add(path);
            }
        }

        private static void FindHorizontalRows(IEnumerable<Tile> tilesToVisit, int maxY, List<Road> roads)
        {
            for (int y = 0; y <= maxY; y++)
            {
                var tilesInRow = tilesToVisit.Where(w => w.Position.Y == y).OrderBy(w => w.Position.X).ToList();

                if (!tilesInRow.Any()) continue;

                Road? road = null;

                foreach (var tile in tilesInRow)
                {
                    bool isIntersecting = tilesToVisit.Any(w => w.Position.X == tile.Position.X && Math.Abs(tile.Position.Y - w.Position.Y) == 1);
                    var isContinuing = road?.Intersection2.Position.X == tile.Position.X - 1;

                    if (isContinuing)
                    {
                        road.Tiles.Add(tile);
                    }

                    if (isIntersecting || !isContinuing)
                    {
                        AddRowIfValid(road, roads);

                        road = new Road(Orientation.Horizontal);
                        road.Tiles.Add(tile);
                    }
                }

                AddRowIfValid(road, roads);
            }
        }

        private static void FindVerticalRows(IEnumerable<Tile> tilesToVisit, int maxX, List<Road> roads)
        {
            for (int x = 0; x <= maxX; x++)
            {
                var tilesInColumn = tilesToVisit.Where(w => w.Position.X == x).OrderBy(w => w.Position.Y).ToList();

                if (!tilesInColumn.Any()) continue;

                Road? road = null;

                foreach (var tile in tilesInColumn)
                {
                    bool isIntersecting = tilesToVisit.Any(w => w.Position.Y == tile.Position.Y && Math.Abs(tile.Position.X - w.Position.X) == 1);
                    var isContinuing = road?.Intersection2.Position.Y == tile.Position.Y - 1;

                    if (isContinuing)
                    {
                        road.Tiles.Add(tile);
                    }
                    
                    if (isIntersecting || !isContinuing)
                    {
                        AddRowIfValid(road, roads);

                        road = new Road(Orientation.Vertical);
                        road.Tiles.Add(tile);
                    }
                }

                AddRowIfValid(road, roads);
            }
        }

        private static void AddRowIfValid(Road? road, IList<Road> roads)
        {
            if (road != null && road.Intersection1 != road.Intersection2)
            {
                roads.Add(road);
                road.SetId();
            }
        }

        public enum Orientation { Horizontal, Vertical };

        class Road
        {
            private static char id = 'A';

            public Road(Orientation orientation)
            {
                this.Orientation = orientation;
            }

            public char Id { get; private set; }

            public List<Tile> Tiles { get; private set; } = new List<Tile>();

            public Tile Intersection1
            {
                get
                {
                    if (Orientation == Orientation.Horizontal)
                    {
                        return Tiles.OrderBy(w => w.Position.X).First();
                    }
                    else
                    {
                        return Tiles.OrderBy(w => w.Position.Y).First();
                    }
                }
            }

            public List<Road> Intersection1Roads { get; } = new List<Road>();

            public Tile Intersection2
            {
                get
                {
                    if (Orientation == Orientation.Horizontal)
                    {
                        return Tiles.OrderBy(w => w.Position.X).Last();
                    }
                    else
                    {
                        return Tiles.OrderBy(w => w.Position.Y).Last();
                    }
                }
            }

            public List<Road> Intersection2Roads { get; } = new List<Road>();

            public Orientation Orientation { get;}

            public void SetId()
            {
                this.Id = id;

                if (++id == '[') id = 'a';

                this.Tiles.ForEach(w => w.CharRepresentation = this.Id);
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

            public World()
            {

            }

            public Tile GetTile(Point position) => this.tileFactory.GetOrCreateInstance(position);

            public Tile GetTile((int x, int y) position) => GetTile(new Point(position.x, position.y));

            public Tile GetTile(int x, int y) => GetTile(new Point(x, y));
        }
    }
}
