using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using SantasToolbox;

namespace _3_Wires
{
    class Program
    {
        static void Main(string[] args)
        {
            using var inputProvider = new InputProvider<string>("Input.txt", GetString);

            //Part1(new List<string> { "R75,D30,R83,U83,L12,D49,R71,U7,L72", "U62,R66,U55,R34,D71,R55,D58,R83" }.GetEnumerator());
            //Part1(new List<string> { "R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51", "U98,R91,D20,R16,D67,R40,U7,R15,U6,R7" }.GetEnumerator());
            //Part1(inputProvider.GetEnumerator());

            //Part2(new List<string> { "R75,D30,R83,U83,L12,D49,R71,U7,L72", "U62,R66,U55,R34,D71,R55,D58,R83" }.GetEnumerator());
            //Part2(new List<string> { "R98,U47,R26,D63,R33,U87,L62,D20,R33,U53,R51", "U98,R91,D20,R16,D67,R40,U7,R15,U6,R7" }.GetEnumerator());
            Part2(inputProvider.GetEnumerator());
        }

        private static void Part1(IEnumerator<string> inputProvider)
        {
            Point centralPoint = new Point(0, 0);
            
            var paths = new List<List<Point>>();

            while(inputProvider.MoveNext())
            {
                paths.Add(CreatePath(inputProvider.Current, centralPoint));
            }

            var crossings = new List<Point>();

            crossings.AddRange(paths[0].Where(w => paths[1].Contains(w)));

            var distances = crossings.Select(w => w.Distance(centralPoint));

            var minDistance = distances.Min();

            Console.WriteLine($"Part 1 complete. Min distance: {minDistance}");
        }

        private static void Part2(IEnumerator<string> inputProvider)
        {
            Point centralPoint = new Point(0, 0);

            var paths = new List<List<Point>>();

            while (inputProvider.MoveNext())
            {
                paths.Add(CreatePath(inputProvider.Current, centralPoint));
            }

            var crossings = new List<Point>();

            crossings.AddRange(paths[0].Where(w => paths[1].Contains(w)));

            var delays = crossings.Select(w =>
            {
                int delayOnFirstWire = 1 + paths[0].FindIndex(ww => ww.Equals(w));
                int delayOnSecondWire = 1 + paths[1].FindIndex(ww => ww.Equals(w));

                return delayOnFirstWire + delayOnSecondWire;
            });

            var minDistance = delays.Min();

            Console.WriteLine($"Part 2 complete. Min distance: {minDistance}");
        }

        private static List<Point> CreatePath(string input, Point centralPoint)
        {
            Point currentPoint = centralPoint;
            var path = new List<Point>();

            var instructions = input.Split(',');

            foreach (var instruction in instructions)
            {
                var distance = int.Parse(instruction.Substring(1));

                var direction = instruction[0];

                Func<Point, Point> incrementor = direction switch
                {
                    'U' => point => point.Up(),
                    'D' => point => point.Down(),
                    'L' => point => point.Left(),
                    'R' => point => point.Right(),
                    _ => throw new ArgumentException("Unknown direction")
                };

                for (int i = 0; i < distance; i++)
                {
                    currentPoint = incrementor(currentPoint);
                    path.Add(currentPoint);
                }
            }

            return path;
        }

        private static bool GetString(string? input, out string value)
        {
            value = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            value = input;

            return true;
        }
    }
}
