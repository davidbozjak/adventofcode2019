using SantasToolbox;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace _21_Springdroid
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputParser = new SingleLineStringInputParser<long>(long.TryParse);
            using var inputProvider = new InputProvider<long>("Input.txt", inputParser.GetValue);

            Part1(inputProvider.ToList());

            Console.WriteLine("Press any key for Part 2");
            Console.ReadKey();

            inputProvider.Reset();

            Part2(inputProvider.ToList());
        }

        private static void Part1(IList<long> programCode)
        {
            string springProgram = "NOT C T\nAND D T\nOR T J\nNOT A T\nOR T J";

            RunSpringboardInstructions(springProgram, false, programCode);
        }

        private static void Part2(IList<long> programCode)
        {
            string a = "NOT C T\nAND D T\nAND H T\nOR T J\n";
            string b = "NOT B T\nAND D T\nOR T J\n";
            string hailMaryPass = "NOT A T\nOR T J\n";

            string springProgram =
                a +
                b +
                hailMaryPass;

            RunSpringboardInstructions(springProgram, true, programCode);
        }

        private static void RunSpringboardInstructions(string springProgram, bool run, IList<long> programCode)
        {
            springProgram = springProgram.Trim();
            var memory = new IntCodeMemory(programCode);
            var computer = new IntCodeComputer();
            var mode = run ? "RUN" : "WALK";

            int instructionCount = 0;

            computer.Run(memory, PassInstructionToSpringdroid, Output);

            long PassInstructionToSpringdroid()
            {
                if (instructionCount < springProgram.Length)
                {
                    return springProgram[instructionCount++];
                }
                else return $"\n{mode}\n"[instructionCount++ - springProgram.Length];
            }
        }

        static char prevChar = (char)0;
        static int pos = 0;
        static int columnCount = 0;

        private static void Output(long output)
        {
            if (output < char.MaxValue)
            {
                char receivedChar = (char)output;

                if (receivedChar == '@')
                {
                    pos = columnCount;
                }

                if (receivedChar == 10)
                {
                    columnCount = 0;
                }

                if (prevChar == 10 && receivedChar == 10)
                {
                    Console.Write(new string(' ', pos));
                    Console.WriteLine("ABCDEFGHI");
                }
                prevChar = receivedChar;

                Console.Write(receivedChar);
                columnCount++;
            }
            else
            {
                Console.WriteLine("Made it accross!");
                Console.WriteLine($"Damage to hull: {output}");
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
}
