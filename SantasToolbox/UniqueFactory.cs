using System;
using System.Collections.Generic;
using System.Linq;

namespace SantasToolbox
{
    public class UniqueFactory<T, U>
        where T:notnull
        where U:notnull
    {
        private readonly Dictionary<T, U> allCreatedInstances = new Dictionary<T, U>();
        private readonly Func<T, U> constructingFunc;

        public UniqueFactory(Func<T, U> constructingFunc)
        {
            this.constructingFunc = constructingFunc;
        }

        public IReadOnlyList<U> AllCreatedInstances => this.allCreatedInstances.Values.ToList().AsReadOnly();

        public U GetOrCreateInstance(T identifier)
        {
            if (!this.allCreatedInstances.ContainsKey(identifier))
            {
                this.allCreatedInstances[identifier] = this.constructingFunc(identifier);
            }

            return this.allCreatedInstances[identifier];
        }

        public void InsertSpecialInstance(T key, U value)
        {
            this.allCreatedInstances[key] = value;
        }
    }
}
