using System;
using System.Collections.Generic;
using SantasToolbox;
using System.Linq;

namespace _6_Orbits
{
    class Program
    {
        static void Main(string[] args)
        {
            using var inputProvider = new InputProvider<(string, string)>("Input.txt", ParseOrbitRelationship);

            Part1(inputProvider);
            
            inputProvider.Reset();

            Part2(inputProvider);

            //Part1(new List<(string, string)>
            //{
            //    ("COM", "B"),
            //    ("B", "C"),
            //    ("C", "D"),
            //    ("D", "E"),
            //    ("E", "F"),
            //    ("B", "G"),
            //    ("G", "H"),
            //    ("D", "I"),
            //    ("E", "J"),
            //    ("J", "K"),
            //    ("K", "L"), }.GetEnumerator());

            //Part2(new List<(string, string)>
            //{
            //    ("COM", "B"),
            //    ("B", "C"),
            //    ("C", "D"),
            //    ("D", "E"),
            //    ("E", "F"),
            //    ("B", "G"),
            //    ("G", "H"),
            //    ("D", "I"),
            //    ("E", "J"),
            //    ("J", "K"),
            //    ("K", "L"),
            //    ("K", "YOU"),
            //    ("I", "SAN"),}.GetEnumerator());
        }

        private static void Part1(IEnumerator<(string, string)> inputProvider)
        {
            var factory = GetAllOrbiters(inputProvider);

            var totalOrbits = factory.AllCreatedInstances.Sum(w => w.TotalOrbiterCount);

            Console.WriteLine("Part1 Done");
            Console.WriteLine($"Total sum of all orbits: {totalOrbits}");
        }

        private static void Part2(IEnumerator<(string, string)> inputProvider)
        {
            var factory = GetAllOrbiters(inputProvider);

            var you = factory.AllCreatedInstances.First(w => w.Name == "YOU");
            var santa = factory.AllCreatedInstances.First(w => w.Name == "SAN");

            var path = FindPath(you, santa, new List<OrbitingBody>());

            Console.WriteLine("Part2 Done");
            Console.WriteLine($"Total transitions needed to orbit same object as Santa: {(path?.Count - 2)?.ToString() ?? "path not found"}");
        }

        private static List<OrbitingBody>? FindPath(OrbitingBody current, OrbitingBody goal, List<OrbitingBody> path)
        {
            if (current == goal)
                return path;

            var nodesToVisit = current.DirectOrbitingBodies.ToList();

            if (current.Orbiting != null)
            {
                nodesToVisit.Add(current.Orbiting);
            }

            foreach (var orbitingBody in nodesToVisit)
            {
                if (path.Contains(orbitingBody))
                    continue;

                var newPath = path.ToList();
                newPath.Add(orbitingBody);

                var successfulPath = FindPath(orbitingBody, goal, newPath);

                if (successfulPath != null)
                    return successfulPath;
            }

            return null;
        }

        private static UniqueFactory<string, OrbitingBody> GetAllOrbiters(IEnumerator<(string, string)> inputProvider)
        {
            var factory = new UniqueFactory<string, OrbitingBody>(w => new OrbitingBody(w));

            while (inputProvider.MoveNext())
            {
                (string orbitingId, string orbiterId) = inputProvider.Current;

                var orbiting = factory.GetOrCreateInstance(orbitingId);
                var orbiter = factory.GetOrCreateInstance(orbiterId);

                orbiting.AddOrbiter(orbiter);
            }

            return factory;
        }

        private static bool ParseOrbitRelationship(string? input, out (string, string) orbitRelationship)
        {
            orbitRelationship = (string.Empty, string.Empty);

            if (string.IsNullOrWhiteSpace(input))
                return false;

            try
            {
                var bodies = input.Split(')');
                orbitRelationship.Item1 = bodies[0];
                orbitRelationship.Item2 = bodies[1];
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
