using System.Collections.Generic;
using System.Linq;

namespace _6_Orbits
{
    class OrbitingBody
    {
        private readonly List<OrbitingBody> orbitingBodies = new List<OrbitingBody>();

        public string Name { get; }

        public OrbitingBody? Orbiting { get; private set; }

        public int DirectOrbiterCount => this.orbitingBodies.Count;

        public int IndirectOrbiterCount => this.orbitingBodies.Sum(w => w.TotalOrbiterCount);

        public int TotalOrbiterCount => this.DirectOrbiterCount + this.IndirectOrbiterCount;

        public IReadOnlyList<OrbitingBody> DirectOrbitingBodies => this.orbitingBodies.AsReadOnly();

        public OrbitingBody(string name)
        {
            this.Name = name;
        }

        public void AddOrbiter(OrbitingBody orbitingBody)
        {
            this.orbitingBodies.Add(orbitingBody);
            orbitingBody.Orbiting = this;
        }
    }
}
