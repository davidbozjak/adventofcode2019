using SantasToolbox;
using System;
using System.Collections.Generic;

namespace _10_Asteroids
{
    class Program
    {
        static void Main(string[] args)
        {
            using var inputProvider = new InputProvider<string>("Input.txt", (string? input, out string value) => { var isnull = string.IsNullOrWhiteSpace(input); value = input ?? string.Empty; return !isnull; });

            Part1(inputProvider.GetEnumerator());

            inputProvider.Reset();

            Part2(inputProvider.GetEnumerator(), (30, 34));
        }

        private static void Part1(IEnumerator<string> inputProvider)
        {
            var asteroidPositions = GetAsteroidPositionsFromInput(inputProvider);

            int maxLineOfSightCount = 0;
            (double x, double y) bestAsteroid = (0, 0);

            foreach (var asteroid in asteroidPositions)
            {
                var lineOfSightCount = GetAllDetectableAsteroidsFromAsteroid(asteroid, asteroidPositions).Count;

                if (lineOfSightCount > maxLineOfSightCount)
                {
                    bestAsteroid = asteroid;
                    maxLineOfSightCount = lineOfSightCount;
                }
            }

            Console.WriteLine("Part 1 Done!");
            Console.WriteLine($"Best asteroid: ({bestAsteroid.x}, {bestAsteroid.y}) with lineOfSight count {maxLineOfSightCount}"); ;
        }

        private static void Part2(IEnumerator<string> inputProvider, (double x, double y) bestShootingPosition)
        {
            var asteroidPositions = GetAsteroidPositionsFromInput(inputProvider);

            double turretAngle = 270;

            for (int destroyedCount = 1; destroyedCount < 205; destroyedCount++)
            {
                var detectableAsteroids = GetAllDetectableAsteroidsFromAsteroid(bestShootingPosition, asteroidPositions);

                (double x, double y) asteroidToDestroy = (-1, -1);
                double minRotationOfTurret = double.MaxValue;
                double newTurretAngle = double.MaxValue;

                foreach (var asteroid in detectableAsteroids)
                {
                    if (asteroid == bestShootingPosition) continue;

                    var relativeX = asteroid.x - bestShootingPosition.x;
                    var relativeY = asteroid.y - bestShootingPosition.y;

                    double angle = WrapTo360(ToDegrees(Math.Atan2(relativeY, relativeX)));

                    double diffFromTurretAngle = WrapTo360(angle - turretAngle);
                    
                    if (diffFromTurretAngle < minRotationOfTurret)
                    {
                        minRotationOfTurret = diffFromTurretAngle;
                        asteroidToDestroy = asteroid;
                        newTurretAngle = angle;
                    }
                }

                bool removed = asteroidPositions.Remove(asteroidToDestroy);
                if (!removed) throw new Exception();
                turretAngle = newTurretAngle + 1e-5;

                Console.WriteLine($"The {destroyedCount}st asteroid to be vaporized is at {asteroidToDestroy}");
            }

            static double ToDegrees(double radians) => radians * 180 / Math.PI;
            static double WrapTo360(double degrees)
            {
                while (degrees < 0) degrees += 360;
                while (degrees > 360) degrees -= 360;
                return degrees;
            }
        }

        private static List<(double x, double y)> GetAllDetectableAsteroidsFromAsteroid((double x, double y) asteroid, List<(double x, double y)> asteroidPositions)
        {
            var detectableAsteroids = new List<(double x, double y)>();

            foreach (var asteroid1 in asteroidPositions)
            {
                if (asteroid == asteroid1) continue;

                (bool isVerticalLine, double k, double n) = CalculateKNBetweenTwoPoints(asteroid, asteroid1);

                Func<double, double> lineOfsight = x => k * x + n;

                bool hasLineOfSight = true;

                foreach (var asteroid2 in asteroidPositions)
                {
                    if (asteroid2 == asteroid) continue;
                    if (asteroid2 == asteroid1) continue;

                    double diff;

                    if (isVerticalLine)
                    {
                        diff = Math.Abs(asteroid2.x - asteroid.x);
                    }
                    else
                    {
                        var y = lineOfsight(asteroid2.x);

                        diff = Math.Abs(y - asteroid2.y);
                    }

                    if (diff < 1e-3)
                    {
                        // is on the line, but is it between the rect?
                        double minX = Math.Min(asteroid.x, asteroid1.x);
                        double maxX = Math.Max(asteroid.x, asteroid1.x);

                        double minY = Math.Min(asteroid.y, asteroid1.y);
                        double maxY = Math.Max(asteroid.y, asteroid1.y);

                        if ((asteroid2.x >= minX && asteroid2.x <= maxX) && (asteroid2.y >= minY && asteroid2.y <= maxY))
                        {
                            hasLineOfSight = false;
                            break;
                        }
                    }
                }

                if (hasLineOfSight) detectableAsteroids.Add(asteroid1);
            }

            return detectableAsteroids;
        }

        private static (bool isVerticalLine, double k, double n) CalculateKNBetweenTwoPoints((double x, double y) asteroid, (double x, double y) asteroid1)
        {
            var isVerticalLine = asteroid.x == asteroid1.x;
            var k = (asteroid.y - asteroid1.y) / (asteroid.x - asteroid1.x);
            var n = asteroid.y - (k * asteroid.x);

            return (isVerticalLine, k, n);
        }

        private static List<(double x, double y)> GetAsteroidPositionsFromInput(IEnumerator<string> inputProvider)
        {
            List<(double x, double y)> asteroidPositions = new List<(double x, double y)>();

            for (int y = 0; inputProvider.MoveNext(); y++)
            {
                var row = inputProvider.Current;
                for (int x = 0; x < row.Length; x++)
                {
                    if (row[x] != '#')
                        continue;

                    asteroidPositions.Add((x, y));
                }
            }

            return asteroidPositions;
        }
    }
}
