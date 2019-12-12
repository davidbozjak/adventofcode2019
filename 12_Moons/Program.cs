using SantasToolbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace _12_Moons
{
    class Program
    {
        static void Main(string[] _)
        {
            using var inputProvider = new InputProvider<Moon>("Input.txt", ParsePosition);

            //Part1(inputProvider.ToList());

            //Console.WriteLine("Press any key to continue to Part2");
            //Console.ReadKey();

            //inputProvider.Reset();

            Part2(inputProvider.ToList());
        }

        private static void Part1(IList<Moon> moons)
        {
            for (int time = 0; time < 1001; time++)
            {
                Console.WriteLine($"After {time} steps");
                foreach (var moon in moons)
                    Console.WriteLine(moon);

                SimulateStepOfUniverse(moons);
            }
        }

        private static void Part2(IList<Moon> moons)
        {
            var originalState = moons.Select(w => new Moon(w.PositionX, w.PositionY, w.PositionZ)).ToList();

            int cycleX, cycleY, cycleZ = 0;

            bool isCycle = false;

            for (cycleX = 1; !isCycle; cycleX++)
            {
                for (int i = 0; i < moons.Count; i++)
                {
                    var moon1 = moons[i];

                    for (int j = i + 1; j < moons.Count; j++)
                    {
                        var moon2 = moons[j];

                        var compareX = moon1.PositionX.CompareTo(moon2.PositionX);

                        moon1.VelocityX -= compareX;
                        moon2.VelocityX += compareX;
                    }
                }

                isCycle = true;

                for (int i = 0; i < moons.Count; i++)
                {
                    var moon = moons[i];
                    moon.UpdatePosition();

                    if (moon.PositionX != originalState[i].PositionX)
                    {
                        isCycle = false;
                    }
                }
            }

            isCycle = false;

            for (cycleY = 1; !isCycle; cycleY++)
            {
                for (int i = 0; i < moons.Count; i++)
                {
                    var moon1 = moons[i];

                    for (int j = i + 1; j < moons.Count; j++)
                    {
                        var moon2 = moons[j];

                        var compareY = moon1.PositionY.CompareTo(moon2.PositionY);

                        moon1.VelocityY -= compareY;
                        moon2.VelocityY += compareY;
                    }
                }

                isCycle = true;

                for (int i = 0; i < moons.Count; i++)
                {
                    var moon = moons[i];
                    moon.UpdatePosition();

                    if (moon.PositionY != originalState[i].PositionY)
                    {
                        isCycle = false;
                    }
                }
            }

            isCycle = false;

            for (cycleZ = 1; !isCycle; cycleZ++)
            {
                for (int i = 0; i < moons.Count; i++)
                {
                    var moon1 = moons[i];

                    for (int j = i + 1; j < moons.Count; j++)
                    {
                        var moon2 = moons[j];

                        var compareZ = moon1.PositionZ.CompareTo(moon2.PositionZ);

                        moon1.VelocityZ -= compareZ;
                        moon2.VelocityZ += compareZ;
                    }
                }

                isCycle = true;

                for (int i = 0; i < moons.Count; i++)
                {
                    var moon = moons[i];
                    moon.UpdatePosition();

                    if (moon.PositionZ != originalState[i].PositionZ)
                    {
                        isCycle = false;
                    }
                }
            }

            Console.WriteLine("Done");
            Console.WriteLine($"Universe cycles X:{cycleX} Y:{cycleY} Z:{cycleZ}");
        }

        private static void SimulateStepOfUniverse(IList<Moon> moons)
        {
            for (int i = 0; i < moons.Count; i++)
            {
                var moon1 = moons[i];

                for (int j = i + 1; j < moons.Count; j++)
                {
                    var moon2 = moons[j];

                    var compareX = moon1.PositionX.CompareTo(moon2.PositionX);
                    var compareY = moon1.PositionY.CompareTo(moon2.PositionY);
                    var compareZ = moon1.PositionZ.CompareTo(moon2.PositionZ);

                    moon1.VelocityX -= compareX;
                    moon2.VelocityX += compareX;

                    moon1.VelocityY -= compareY;
                    moon2.VelocityY += compareY;

                    moon1.VelocityZ -= compareZ;
                    moon2.VelocityZ += compareZ;
                }
            }

            foreach (var moon in moons)
                moon.UpdatePosition();
        }

        private static bool ParsePosition(string? input, out Moon value)
        {
            value = new Moon(0, 0, 0);

            if (string.IsNullOrWhiteSpace(input))
                return false;

            try
            {
                var numRegex = new Regex(@"-?\d+");
                var matches = numRegex.Matches(input).Select(w => int.Parse(w.Value)).ToArray();

                value = new Moon(matches[0], matches[1], matches[2]);
            }
            catch
            {
                return false;
            }

            return true;
        }

        class Moon
        {
            public int PositionX { get; set; }
            public int PositionY { get; set; }
            public int PositionZ { get; set; }

            public int VelocityX { get; set; }
            public int VelocityY { get; set; }
            public int VelocityZ { get; set; }

            public int PotentialEnergy => Math.Abs(PositionX) + Math.Abs(PositionY) + Math.Abs(PositionZ);

            public int KineticEnergy => Math.Abs(VelocityX) + Math.Abs(VelocityY) + Math.Abs(VelocityZ);

            public int Energy => PotentialEnergy * KineticEnergy;

            public Moon(int x, int y, int z)
            {
                this.PositionX = x;
                this.PositionY = y;
                this.PositionZ = z;
            }

            public void UpdatePosition()
            {
                this.PositionX += this.VelocityX;
                this.PositionY += this.VelocityY;
                this.PositionZ += this.VelocityZ;
            }

            public override string ToString()
            {
                return $"pos=<x= {PositionX}, y= {PositionY}, z= {PositionZ}>, vel=<x= {VelocityX}, y= {VelocityY}, z= {VelocityZ}> Total energy {Energy} ({PotentialEnergy} * {KineticEnergy})";
            }
        }
    }
}
