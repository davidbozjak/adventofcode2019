using SantasToolbox;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _21_Springdroid
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputParser = new SingleLineStringInputParser<long>(long.TryParse);
            using var inputProvider = new InputProvider<long>("Input.txt", inputParser.GetValue);

            Part1(inputProvider.ToList());

            //inputProvider.Reset();

            //Part2(inputProvider.ToList());
        }

        private static void Part1(IList<long> programCode)
        {
            var memory = new IntCodeMemory(programCode);
            var computer = new IntCodeComputer();

            string instruction = "NOT C T\nAND D T\nOR T J\nNOT A T\nOR T J";
            int instructionCount = 0;

            computer.Run(memory, PassInstructionToSpringdroid, Output);

            long PassInstructionToSpringdroid()
            {
                if (instructionCount < instruction.Length)
                {
                    return instruction[instructionCount++];
                }
                else return "\nWALK\n"[instructionCount++ - instruction.Length];
            }

            void Output(long output)
            {
                if (output < char.MaxValue)
                {
                    Console.Write((char)output);
                }
                else
                {
                    Console.WriteLine("Made it accross! Part 1 Complete.");
                    Console.WriteLine($"Damage to hull: {output}");
                }
            }
        }

        private static void Part2(IList<long> programCode)
        {
            var memory = new IntCodeMemory(programCode);
            var computer = new IntCodeComputer();
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
}
