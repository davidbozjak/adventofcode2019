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
        }

        private static void Part1(IEnumerator<(string, string)> inputProvider)
        {
            var factory = new UniqueFactory<OrbitingBody, string>(w => w.Name, w => new OrbitingBody(w));

            while (inputProvider.MoveNext())
            {
                (string orbitingId, string orbiterId) = inputProvider.Current;

                var orbiting = factory.GetOrCreateInstance(orbitingId);
                var orbiter = factory.GetOrCreateInstance(orbiterId);

                orbiting.AddOrbiter(orbiter);
            }

            var totalOrbits = factory.AllCreatedInstances.Sum(w => w.TotalOrbiterCount);

            Console.WriteLine("Part1 Done");
            Console.WriteLine($"Total sum of all orbits: {totalOrbits}");
        }

        class UniqueFactory<T, U>
            where U : IComparable
        {
            private readonly List<T> allCreatedInstances = new List<T>();
            private readonly Func<T, U> identifierFunc;
            private readonly Func<U, T> constructingFunc;

            public UniqueFactory(Func<T, U> identifierFunc, Func<U, T> constructingFunc)
            {
                this.identifierFunc = identifierFunc;
                this.constructingFunc = constructingFunc;
            }

            public IReadOnlyList<T> AllCreatedInstances => this.allCreatedInstances.AsReadOnly();

            public T GetOrCreateInstance(U identifier)
            {
                var instance = this.allCreatedInstances.FirstOrDefault(w => this.identifierFunc(w).CompareTo(identifier) == 0);

                if (instance != null) return instance;

                instance = this.constructingFunc(identifier);

                allCreatedInstances.Add(instance);

                return instance;
            }
        }

        class OrbitingBody
        {
            private readonly List<OrbitingBody> orbitingBodies = new List<OrbitingBody>();

            public string Name { get; }

            public int DirectOrbiterCount => this.orbitingBodies.Count;

            public int IndirectOrbiterCount => this.orbitingBodies.Sum(w => w.TotalOrbiterCount);

            public int TotalOrbiterCount => this.DirectOrbiterCount + this.IndirectOrbiterCount;

            public OrbitingBody(string name)
            {
                this.Name = name;
            }

            public void AddOrbiter(OrbitingBody orbitingBody)
            {
                this.orbitingBodies.Add(orbitingBody);
            }
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
